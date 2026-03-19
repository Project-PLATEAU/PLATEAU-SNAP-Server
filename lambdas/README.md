# Geo Lambda

Multi-function Lambda service for geospatial processing (orthographic transformation, roof extraction, texture building).

## Building for Lambda

**Critical**: Use `--provenance=false` flag to avoid manifest issues:

**重要**: マニフェストの問題を回避するため、`--provenance=false`フラグを使用してください：

```bash
docker buildx build \
    --platform linux/arm64 \
    --provenance=false \
    --tag xxx.dkr.ecr.ap-northeast-1.amazonaws.com/geo-lambda:latest \
    --push .
```

## Functions

### `ortho_transform`

**Description:** PLATEAU SNAP アプリ撮影画像を正射投影された画像に変換

**Environment Variables:**

- `OUTPUT_S3_BUCKET` *(optional)* — output bucket
- `BUFFER_PIXELS` *(optional, default: 100)* — pixel buffer

---

### `roof_extraction`

**Description:** PLATEAU-Ortho より画像をダウンロードし、対象の屋根面を内包する画像を生成

**Environment Variables:**

- `OUTPUT_S3_BUCKET` *(required)* — output bucket
- `ROOF_EXTRACTION_ZOOM` *(optional, default: 18)* — tile zoom
- `BUFFER_BASE_METERS` *(optional, default: 20)* — fallback buffer size in meters for small polygons
- `BUFFER_MIN_METERS` *(optional, default: 15)* — minimum buffer size in meters
- `BUFFER_MAX_METERS` *(optional, default: 35)* — maximum buffer size in meters
- `BUFFER_SIZE_FACTOR` *(optional, default: 0.15)* — percentage of building size used for adaptive buffer (0.15 = 15%)

---

### `texture_building`

**Description:** 対象面を UV 展開し、テクスチャとして表示するためのデータを生成

**Environment Variables:**

- `OUTPUT_S3_BUCKET` *(optional)* — output bucket

## Architecture

- **Runtime**: Python 3.12 on ARM64 (AWS Graviton)
- **Deployment**: Single Docker image, multiple Lambda functions
- **VPC**: Private subnet with S3/internet access as needed
