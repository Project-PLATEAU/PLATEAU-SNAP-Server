import requests_mock
from src.roof_extraction.handler import lambda_handler

def test_roof_extraction_success(moto_s3, roof_bucket_env):
    moto_s3.create_bucket(
        Bucket="test-bucket",
        CreateBucketConfiguration={"LocationConstraint": "ap-northeast-1"},
    )

    body = {
        "geometry": "POLYGON((139.7671 35.6812, 139.7672 35.6812, 139.7672 35.6813, 139.7671 35.6813, 139.7671 35.6812))"
    }

    with requests_mock.Mocker() as m:
        # 1x1 PNG (intentionally tiny—our code resizes/falls back)
        png_1x1 = b'\x89PNG\r\n\x1a\n\x00\x00\x00\rIHDR\x00\x00\x01\x00\x00\x00\x01\x00\x08\x02\x00\x00\x00\x90wS\xde\x00\x00\x00\x0cIDATx\x9cc\x00\x01\x00\x00\x05\x00\x01\r\n-\xb4\x00\x00\x00\x00IEND\xaeB`\x82'
        m.get(requests_mock.ANY, content=png_1x1)
        result = lambda_handler(body, None)

    assert result["statusCode"] == 200
    assert "path" in result["body"]
    assert "coordinates" in result["body"]

def test_roof_extraction_missing_geometry():
    body = {}
    result = lambda_handler(body, None)
    assert result["statusCode"] == 400

def test_roof_extraction_missing_bucket_env():
    import os
    os.environ.pop("OUTPUT_S3_BUCKET", None)
    body = {"geometry": "POLYGON((139.7671 35.6812, 139.7672 35.6812, 139.7672 35.6813, 139.7671 35.6813, 139.7671 35.6812))"}
    result = lambda_handler(body, None)
    assert result["statusCode"] == 500
