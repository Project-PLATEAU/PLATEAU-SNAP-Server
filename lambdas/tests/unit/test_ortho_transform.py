from src.ortho_transform.handler import lambda_handler

def test_ortho_transform_success(moto_s3, input_output_buckets, small_image):
    input_bucket, output_bucket = input_output_buckets
    moto_s3.put_object(Bucket=input_bucket, Key="test.png", Body=small_image)

    body = {
        "path": f"s3://{input_bucket}/test.png",
        "coordinates": "POLYGON ((214.29571533203125 2169.7783203125, 1035.5516357421875 2107.105224609375, 914.1983642578125 935.53662109375, 688.4185791015625 855.71826171875, 698.14013671875 1031.571533203125, 435.3450012207031 953.945068359375, 435.09722900390625 1230.9443359375, 252.11648559570312 1192.6239013671875, 214.29571533203125 2169.7783203125))",
        "geometry": "POLYGON Z ((-5945.399999974428 -35942.9400001737 4.15,-5949.349999974228 -35951.140000173094 4.15,-5949.349999974228 -35951.140000173094 18.196,-5947.768999974996 -35947.85800017374 18.196,-5947.768999974996 -35947.85800017374 15.8,-5946.260999974484 -35944.7270001749 15.8,-5946.260999974484 -35944.7270001749 12.6,-5945.399999974428 -35942.9400001737 12.6,-5945.399999974428 -35942.9400001737 4.15))"
    }

    result = lambda_handler(body, None)
    assert result["statusCode"] == 200
    assert "path" in result["body"]
    assert "coordinates" in result["body"]

def test_ortho_transform_missing_params():
    body = {"path": "s3://bucket/test.png"}
    result = lambda_handler(body, None)
    assert result["statusCode"] == 400

def test_ortho_transform_s3_not_found(input_output_buckets):
    input_bucket, _ = input_output_buckets  # bucket exists but object missing
    body = {
        "path": f"s3://{input_bucket}/missing.png",
        "coordinates": "POLYGON((10 10, 50 10, 50 50, 10 50, 10 10))",
        "geometry": "POLYGON Z((0 0 0, 1 0 0, 1 1 0, 0 1 0, 0 0 0))",
    }
    result = lambda_handler(body, None)
    assert result["statusCode"] == 404

def test_ortho_transform_success(moto_s3, input_output_buckets, large_image, memory_monitor):
    input_bucket, output_bucket = input_output_buckets
    moto_s3.put_object(Bucket=input_bucket, Key="test.png", Body=large_image)

    body = {
        "path": f"s3://{input_bucket}/test.png",
        "coordinates": "POLYGON ((214.29571533203125 2169.7783203125, 1035.5516357421875 2107.105224609375, 914.1983642578125 935.53662109375, 688.4185791015625 855.71826171875, 698.14013671875 1031.571533203125, 435.3450012207031 953.945068359375, 435.09722900390625 1230.9443359375, 252.11648559570312 1192.6239013671875, 214.29571533203125 2169.7783203125))",
        "geometry": "POLYGON Z ((-5945.399999974428 -35942.9400001737 4.15,-5949.349999974228 -35951.140000173094 4.15,-5949.349999974228 -35951.140000173094 18.196,-5947.768999974996 -35947.85800017374 18.196,-5947.768999974996 -35947.85800017374 15.8,-5946.260999974484 -35944.7270001749 15.8,-5946.260999974484 -35944.7270001749 12.6,-5945.399999974428 -35942.9400001737 12.6,-5945.399999974428 -35942.9400001737 4.15))"
    }

    result = lambda_handler(body, None)
    mem_used_mb = memory_monitor[0] / 1024 / 1024
    print(f"Peak memory: {mem_used_mb:.1f}MB")
    
    assert result["statusCode"] == 200
    assert "path" in result["body"]
    assert "coordinates" in result["body"]
    assert mem_used_mb < 500, f"Used {mem_used_mb:.1f}MB (limit: 500MB)"