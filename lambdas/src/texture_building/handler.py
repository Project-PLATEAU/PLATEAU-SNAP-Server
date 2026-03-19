"""
Crop a facade from a source image (stored in S3) and return the cropped
texture plus its UV map.
"""
from __future__ import annotations

from os import environ
import uuid
from datetime import datetime, timezone
from typing import List, Tuple, TYPE_CHECKING
from urllib.parse import urlparse

from ..shared.response_formatters import _resp
from ..shared.s3_utils import download_from_s3, upload_png_to_s3
from ..shared.decorators import api_handler, ApiError
from ..shared.logger import get_logger

from ..shared.lazy_imports import get_shapely_wkt, get_shapely_polygon


if TYPE_CHECKING:
    import numpy as np

logger = get_logger(__name__)



@api_handler
def lambda_handler(body, _context):
    logger.info("λ start (uv) - %s", {k: body.get(k) for k in ("path", "coordinates")})
    path: str | None = body.get("path")
    wkt: str | None = body.get("coordinates")
    if not (path and wkt):
        logger.warning("Missing param(s): path=%s wkt=%s", bool(path), bool(wkt))
        raise ApiError(400, "Missing required parameters: path, coordinates")

    img = download_from_s3(path)
    cropped, uv_wkt = crop_and_generate_uvs(img, wkt)

    default_bucket = urlparse(path).netloc
    bucket = environ.get("OUTPUT_S3_BUCKET", default_bucket)
    folder = datetime.now(timezone.utc).strftime("%Y-%m-%d")
    key = f"temp/{folder}/{uuid.uuid4()}_texture.png"
    out_path = upload_png_to_s3(cropped, bucket, key)

    return _resp(200, "success", path=out_path, texture_coordinates=uv_wkt)


def crop_and_generate_uvs(image: "np.ndarray", pixel_wkt: str) -> Tuple["np.ndarray", str]:
    Polygon = get_shapely_polygon()
    wkt_mod = get_shapely_wkt()

    logger.debug("Parsing pixel-space WKT: %s", pixel_wkt)
    poly = wkt_mod.loads(pixel_wkt)
    if not isinstance(poly, Polygon):
        logger.warning("Non-polygon WKT received")
        raise ApiError(422, "coordinates must be a WKT POLYGON")

    img_h, img_w = image.shape[:2]
    min_x, min_y, max_x, max_y = map(int, poly.bounds)
    if not (0 <= min_x < max_x <= img_w and 0 <= min_y < max_y <= img_h):
        logger.warning("Polygon (%s) outside image bounds %dx%d", poly.bounds, img_w, img_h)
        raise ApiError(422, "Polygon is outside image bounds")

    crop = image[min_y:max_y, min_x:max_x]
    crop_h, crop_w = crop.shape[:2]

    uv_coords: List[Tuple[float, float]] = []
    for px, py in poly.exterior.coords:
        u = (px - min_x) / crop_w
        v = 1.0 - ((py - min_y) / crop_h)
        uv_coords.append((u, v))

    uv_wkt = Polygon(uv_coords).wkt
    return crop, uv_wkt