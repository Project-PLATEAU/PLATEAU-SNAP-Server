from src.texture_building.handler import lambda_handler

def test_texture_building_success(moto_s3, input_output_buckets, png_factory):
    input_bucket, output_bucket = input_output_buckets
    img_bytes, _ = png_factory(200, 200, color=255)
    moto_s3.put_object(Bucket=input_bucket, Key="test.png", Body=img_bytes)

    body = {
        "path": f"s3://{input_bucket}/test.png",
        "coordinates": "POLYGON((20 20, 80 20, 80 80, 20 80, 20 20))",
    }
    result = lambda_handler(body, None)

    assert result["statusCode"] == 200
    assert "path" in result["body"]
    assert "texture_coordinates" in result["body"]

def test_texture_building_missing_params():
    body = {"path": "s3://bucket/test.png"}
    result = lambda_handler(body, None)
    assert result["statusCode"] == 400

def test_texture_building_polygon_out_of_bounds(moto_s3, input_output_buckets, png_factory):
    input_bucket, _ = input_output_buckets
    img_bytes, _ = png_factory(200, 200, color=255)
    moto_s3.put_object(Bucket=input_bucket, Key="test.png", Body=img_bytes)

    body = {
        "path": f"s3://{input_bucket}/test.png",
        "coordinates": "POLYGON((500 500, 600 500, 600 600, 500 600, 500 500))",
    }
    result = lambda_handler(body, None)
    assert result["statusCode"] == 422
