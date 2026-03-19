from __future__ import annotations

import os
from urllib.parse import urlparse, ParseResult
from typing import NoReturn, TYPE_CHECKING

import boto3
from botocore.exceptions import ClientError

from ..shared.decorators import ApiError
from ..shared.lazy_imports import get_numpy, get_cv2

if TYPE_CHECKING:
    import numpy as np


def _parse_s3_uri(s3_uri: str) -> ParseResult:
    p = urlparse(s3_uri)

    # If there's no scheme, treat as bucket/key and add s3://
    if not p.scheme:
        s3_uri = f"s3://{s3_uri}"
        p = urlparse(s3_uri)

    if p.scheme.lower() != "s3":
        raise ApiError(400, f"Invalid S3 URI scheme: '{p.scheme}'")

    if not p.netloc:
        raise ApiError(400, "S3 URI must include a bucket name")

    if not p.path or p.path == "/":
        raise ApiError(400, "S3 URI must include an object key")

    return p


def _s3_client():
    return boto3.client("s3", region_name=os.getenv("AWS_REGION", "ap-northeast-1"))


def download_from_s3(s3_uri: str) -> "np.ndarray":
    np = get_numpy()
    cv2 = get_cv2()
    
    p = _parse_s3_uri(s3_uri)
    bucket = p.netloc
    key = p.path.lstrip("/")

    s3 = _s3_client()
    try:
        resp = s3.get_object(Bucket=bucket, Key=key)
    except ClientError as err:
        _handle_s3_client_error(err, bucket, key)

    arr = np.frombuffer(resp["Body"].read(), np.uint8)
    img = cv2.imdecode(arr, cv2.IMREAD_COLOR)
    if img is None:
        raise ApiError(422, f"Failed to decode image data from {s3_uri}")
    return img


def upload_png_to_s3(img: "np.ndarray", bucket: str, key: str) -> str:
    cv2 = get_cv2()
    
    ok, buf = cv2.imencode(".png", img)
    if not ok:
        raise ApiError(422, "PNG encoding failed")

    s3 = _s3_client()
    try:
        s3.put_object(
            Bucket=bucket,
            Key=key,
            Body=buf.tobytes(),
            ContentType="image/png",
        )
    except ClientError as err:
        _handle_s3_client_error(err, bucket, key)

    return f"s3://{bucket}/{key}"


def _handle_s3_client_error(err: ClientError, bucket: str, key: str) -> NoReturn:
    error_code = err.response.get("Error", {}).get("Code", "")
    if error_code == "NoSuchKey":
        raise ApiError(404, f"Object '{key}' not found in bucket '{bucket}'") from err
    if error_code == "AccessDenied":
        raise ApiError(403, f"Access denied to s3://{bucket}/{key}") from err
    raise ApiError(502, f"S3 error ({error_code}): {err}") from err