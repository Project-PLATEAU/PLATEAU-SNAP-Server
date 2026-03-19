import json
from src.texture_building.handler import lambda_handler

def test_texture_building_large_image(moto_s3, input_output_buckets, output_bucket_env, large_image, memory_monitor):
    input_bucket, output_bucket = input_output_buckets
    moto_s3.put_object(Bucket=input_bucket, Key="big.png", Body=large_image)

    body = {
        "path": f"s3://{input_bucket}/big.png",
        "coordinates": "POLYGON((1500 1500, 2500 1500, 2500 2500, 1500 2500, 1500 2500, 1500 1500))",
    }
    res = lambda_handler(body, None)
    mem_used_mb = memory_monitor[0] / 1024 / 1024
    print(f"Peak memory: {mem_used_mb:.1f}MB")

    assert res["statusCode"] == 200
    body_dict = res["body"] if isinstance(res["body"], dict) else json.loads(res["body"])
    path = body_dict["path"]
    assert path.startswith(f"s3://{output_bucket}/")
    assert mem_used_mb < 500, f"Used {mem_used_mb:.1f}MB (limit: 500MB)"