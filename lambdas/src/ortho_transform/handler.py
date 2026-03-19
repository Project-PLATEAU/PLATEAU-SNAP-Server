"""
Warp an image defined by a WKT POLYGON, store the warped PNG in S3, and return its path along with the building coords in the new image.
"""

from __future__ import annotations
from os import environ
import uuid
from datetime import datetime, timezone
import json
from urllib.parse import urlparse
from typing import TYPE_CHECKING

from ..shared.response_formatters import _resp
from ..shared.s3_utils import download_from_s3, upload_png_to_s3
from ..shared.decorators import api_handler, ApiError
from .geometry_processing import wkt_polygon_to_coords
from .image_processing import rectify_facade
from ..shared.lazy_imports import get_shapely_polygon

from ..shared.logger import get_logger
logger = get_logger(__name__)

if TYPE_CHECKING:
    from shapely.geometry import Polygon


@api_handler
def lambda_handler(body, _context):
    logger.info("λ start (ortho) - %s", json.dumps({k: body.get(k) for k in ("path", "coordinates", "geometry")}, ensure_ascii=False))

    path: str | None = body.get("path")
    coordinates: str | None = body.get("coordinates")
    geometry: str | None = body.get("geometry")

    missing = [name for name, value in {"path": path, "coordinates": coordinates, "geometry": geometry}.items() if not value]
    if missing:
        logger.error("Missing required parameter(s): %s", ', '.join(missing))
        raise ApiError(400, f"Missing required parameter(s): {', '.join(missing)}")

    img = download_from_s3(path)
    coordinate_pts = wkt_polygon_to_coords(coordinates)
    geometry_pts = wkt_polygon_to_coords(geometry)

    warped_img, warped_pts = rectify_facade(img, coordinate_pts, geometry_pts)
    default_bucket = urlparse(path).netloc
    bucket = environ.get("OUTPUT_S3_BUCKET", default_bucket)
    folder = datetime.now(timezone.utc).strftime("%Y-%m-%d")
    key = f"temp/{folder}/{uuid.uuid4()}_transformed.png"
    out_path = upload_png_to_s3(warped_img, bucket, key)
    
    Polygon = get_shapely_polygon()
    out_wkt = Polygon(warped_pts).wkt
    logger.debug("λ done - out_wkt=%s", out_wkt[:60])
    return _resp(200, "success", path=out_path, coordinates=out_wkt)
