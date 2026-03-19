"""
Mosaic-and-crop an AOI from Plateau ortho, then upload the PNG to S3.
"""
from __future__ import annotations

import math
from os import environ
import uuid
from datetime import datetime, timezone
from typing import List, Tuple, TYPE_CHECKING

import requests
from requests.adapters import HTTPAdapter, Retry

from ..shared.response_formatters import _resp
from ..shared.s3_utils import upload_png_to_s3
from ..shared.decorators import api_handler, ApiError
from ..shared.logger import get_logger
from ..shared.lazy_imports import (
    get_numpy,
    get_cv2,
    get_mercantile,
    get_shapely_wkt,
    get_shapely_polygon,
)

if TYPE_CHECKING:
    import numpy as np
    from shapely.geometry import Polygon

logger = get_logger(__name__)

TILE_SIZE = 256
URL_TEMPLATE = (
    "https://api.plateauview.mlit.go.jp/tiles/plateau-ortho-2023/{z}/{x}/{y}.png"
)
ZOOM_LEVEL = int(environ.get("ROOF_EXTRACTION_ZOOM", "18"))
EARTH_RADIUS = 6378137


@api_handler
def lambda_handler(body, _context):
    logger.info("λ start (plateau) - geometry: %s", body.get("geometry"))
    wkt: str | None = body.get("geometry")
    if not wkt:
        logger.warning("Missing geometry param")
        raise ApiError(400, "Missing required parameters: geometry")

    bucket = environ.get("OUTPUT_S3_BUCKET")
    if not bucket:
        logger.error("OUTPUT_S3_BUCKET env var not set")
        raise RuntimeError("OUTPUT_S3_BUCKET env var not set")

    coords = parse_wkt_polygon(wkt)
    session = _build_session()
    logger.info("Calculated %d AOI vertices; zoom=%d", len(coords), ZOOM_LEVEL)
    crop_img, pixel_vertices = mosaic_and_crop(coords, ZOOM_LEVEL, session)

    folder = datetime.now(timezone.utc).strftime("%Y-%m-%d")
    key = f"temp/{folder}/{uuid.uuid4()}.png"
    s3_path = upload_png_to_s3(crop_img, bucket, key)

    out_wkt = coords_to_wkt(pixel_vertices)

    return _resp(200, "success", path=s3_path, coordinates=out_wkt)


def parse_wkt_polygon(wkt: str) -> List[Tuple[float, float]]:
    wkt_mod = get_shapely_wkt()
    logger.debug("Parsing WKT: %s", wkt)
    try:
        poly = wkt_mod.loads(wkt)
        if poly.geom_type != "Polygon":
            raise ValueError
        return list(poly.exterior.coords)
    except Exception:
        raise ValueError("Invalid WKT POLYGON string")


def _calculate_tile_bounds(lonlat_vertices: List[Tuple[float, float]], zoom: int) -> Tuple[int, int, int, int]:
    mercantile = get_mercantile()
    tiles = [mercantile.tile(lon, lat, zoom) for lon, lat in lonlat_vertices]
    min_x = min(t.x for t in tiles)
    max_x = max(t.x for t in tiles)
    min_y = min(t.y for t in tiles)
    max_y = max(t.y for t in tiles)
    return (min_x, min_y, max_x, max_y)


def _create_mosaic(bounds: Tuple[int, int, int, int], zoom: int, session: requests.Session) -> "np.ndarray":
    np = get_numpy()
    min_x, min_y, max_x, max_y = bounds
    width_in_tiles = max_x - min_x + 1
    height_in_tiles = max_y - min_y + 1
    mosaic = np.zeros(
        (height_in_tiles * TILE_SIZE, width_in_tiles * TILE_SIZE, 3),
        dtype=np.uint8,
    )

    for x in range(min_x, max_x + 1):
        for y in range(min_y, max_y + 1):
            tile_img = fetch_tile(session, zoom, x, y)
            row_start = (y - min_y) * TILE_SIZE
            col_start = (x - min_x) * TILE_SIZE
            mosaic[row_start : row_start + TILE_SIZE, col_start : col_start + TILE_SIZE] = tile_img
    return mosaic


def _calculate_adaptive_buffer(lonlat_vertices: List[Tuple[float, float]]) -> float:
    """Calculate buffer based on building size"""
    BASE_BUFFER = float(environ.get('BUFFER_BASE_METERS', '20'))
    MIN_BUFFER = float(environ.get('BUFFER_MIN_METERS', '15'))
    MAX_BUFFER = float(environ.get('BUFFER_MAX_METERS', '35'))
    SIZE_FACTOR = float(environ.get('BUFFER_SIZE_FACTOR', '0.15'))
    
    if len(lonlat_vertices) < 3:
        return BASE_BUFFER
    
    lons = [lon for lon, lat in lonlat_vertices]
    lats = [lat for lon, lat in lonlat_vertices]
    lon_range = max(lons) - min(lons)
    lat_range = max(lats) - min(lats)
    avg_lat = sum(lats) / len(lats)
    lon_meters = lon_range * 111320 * math.cos(math.radians(avg_lat))
    lat_meters = lat_range * 110540
    building_size = max(lon_meters, lat_meters)
    adaptive_buffer = max(MIN_BUFFER, min(MAX_BUFFER, building_size * SIZE_FACTOR))
    return adaptive_buffer


def _convert_geo_to_mosaic_pixels(
    lonlat_vertices: List[Tuple[float, float]], zoom: int, min_tile_x: int, min_tile_y: int
) -> List[Tuple[int, int]]:
    mosaic_pixels = []
    for lon, lat in lonlat_vertices:
        t_x, t_y, px, py = lonlat_to_pixel(lon, lat, zoom)
        mosaic_px = (t_x - min_tile_x) * TILE_SIZE + px
        mosaic_py = (t_y - min_tile_y) * TILE_SIZE + py
        mosaic_pixels.append((mosaic_px, mosaic_py))
    return mosaic_pixels


def mosaic_and_crop(
    lonlat_vertices: List[Tuple[float, float]], zoom: int, session: requests.Session
) -> Tuple["np.ndarray", List[Tuple[int, int]]]:
    np = get_numpy()
    cv2 = get_cv2()
    
    bounds = _calculate_tile_bounds(lonlat_vertices, zoom)
    logger.info("Tile bounds x:[%d‑%d] y:[%d‑%d]", *bounds)
    min_x, min_y, max_x, max_y = bounds
    mosaic = _create_mosaic(bounds, zoom, session)
    logger.debug("Mosaic shape=%s", mosaic.shape)
    mosaic_pixels = _convert_geo_to_mosaic_pixels(lonlat_vertices, zoom, min_x, min_y)

    contour = np.array(mosaic_pixels, dtype=np.int32)
    x, y, w, h = cv2.boundingRect(contour)

    lat_center = sum(lat for _, lat in lonlat_vertices) / len(lonlat_vertices)
    m_per_px = (2 * math.pi * EARTH_RADIUS * math.cos(math.radians(lat_center))) / (2**zoom * TILE_SIZE)
    buffer_meters = _calculate_adaptive_buffer(lonlat_vertices)
    buf_px = int(buffer_meters / m_per_px)
    logger.info("Adaptive buffer=%.1fm (≈%d px)", buffer_meters, buf_px)

    x1, y1 = max(0, x - buf_px), max(0, y - buf_px)
    x2, y2 = min(mosaic.shape[1], x + w + buf_px), min(mosaic.shape[0], y + h + buf_px)

    crop = mosaic[y1:y2, x1:x2]
    shifted_pixels = [(px - x1, py - y1) for px, py in mosaic_pixels]

    return crop, shifted_pixels


def coords_to_wkt(pixels: List[Tuple[int, int]]) -> str:
    Polygon = get_shapely_polygon()
    return Polygon(pixels).wkt


def lonlat_to_pixel(lon: float, lat: float, z: int) -> Tuple[int, int, int, int]:
    lat_rad = math.radians(lat)
    n = 2**z
    xtile_f = (lon + 180.0) / 360.0 * n
    ytile_f = (1 - math.log(math.tan(lat_rad) + 1 / math.cos(lat_rad)) / math.pi) / 2 * n

    tx, ty = int(xtile_f), int(ytile_f)
    px = int((xtile_f - tx) * TILE_SIZE)
    py = int((ytile_f - ty) * TILE_SIZE)
    return tx, ty, px, py


def _build_session() -> requests.Session:
    s = requests.Session()
    s.headers["User-Agent"] = "serverless-tile-processor/1.1 (AWS Lambda; Snap)"

    retry_strategy = Retry(
        total=2,
        backoff_factor=0.3,
        status_forcelist=(500, 502, 503, 504, 429),
        allowed_methods=frozenset(["GET"]),
    )

    adapter = HTTPAdapter(
        max_retries=retry_strategy,
        pool_connections=10,
        pool_maxsize=10
    )

    s.mount("https://", adapter)
    return s


def fetch_tile(session: requests.Session, z: int, x: int, y: int) -> "np.ndarray":
    np = get_numpy()
    cv2 = get_cv2()
    
    url = URL_TEMPLATE.format(z=z, x=x, y=y)
    try:
        with session.get(url, timeout=(5, 10)) as resp:
            resp.raise_for_status()
            img = cv2.imdecode(np.frombuffer(resp.content, np.uint8), cv2.IMREAD_COLOR)
            if img is None:
                logger.error("Tile %d/%d/%d decode fail; substituting blank tile", z, x, y)
                return np.zeros((TILE_SIZE, TILE_SIZE, 3), dtype=np.uint8)

            if img.shape[0] != TILE_SIZE or img.shape[1] != TILE_SIZE:
                try:
                    img = cv2.resize(img, (TILE_SIZE, TILE_SIZE), interpolation=cv2.INTER_AREA)
                except Exception as e:
                    logger.error("Tile %d/%d/%d resize fail (%s); substituting blank tile", z, x, y, e)
                    return np.zeros((TILE_SIZE, TILE_SIZE, 3), dtype=np.uint8)

            return img

    except requests.RequestException as exc:
        logger.error("Tile %d/%d/%d HTTP error: %s; substituting blank tile", z, x, y, exc)
        return np.zeros((TILE_SIZE, TILE_SIZE, 3), dtype=np.uint8)
