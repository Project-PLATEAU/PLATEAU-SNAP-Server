import requests_mock
from src.roof_extraction.handler import lambda_handler

def test_roof_extraction_multiple_tiles(moto_s3, roof_bucket_env):
    moto_s3.create_bucket(
        Bucket="test-bucket",
        CreateBucketConfiguration={"LocationConstraint": "ap-northeast-1"},
    )

    # span multiple tiles at z=18
    body = {
        "geometry": "POLYGON((139.7668 35.6810, 139.7685 35.6810, 139.7685 35.6822, 139.7668 35.6822, 139.7668 35.6810))"
    }

    # Mock valid 256x256 tile
    import numpy as np, cv2
    img = (np.ones((256, 256, 3), dtype=np.uint8) * 128)
    ok, buf = cv2.imencode(".png", img); assert ok
    tile_png = buf.tobytes()

    with requests_mock.Mocker() as m:
        m.get(requests_mock.ANY, content=tile_png)
        res = lambda_handler(body, None)

    assert res["statusCode"] == 200
    assert "path" in res["body"]
    assert "coordinates" in res["body"]
