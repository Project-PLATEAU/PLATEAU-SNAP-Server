import os
import uuid
import threading
import time
import numpy as np
import cv2
import pytest
from moto import mock_aws
import psutil

@pytest.fixture(autouse=True)
def fixed_uuid(monkeypatch):
    monkeypatch.setattr("uuid.uuid4", lambda: uuid.UUID("00000000-0000-0000-0000-000000000000"))
    yield


@pytest.fixture
def output_bucket_env():
    os.environ["OUTPUT_S3_BUCKET"] = "output-bucket"
    yield
    os.environ.pop("OUTPUT_S3_BUCKET", None)

@pytest.fixture
def roof_bucket_env():
    os.environ["OUTPUT_S3_BUCKET"] = "test-bucket"
    yield
    os.environ.pop("OUTPUT_S3_BUCKET", None)

@pytest.fixture(scope="session")
def png_factory():
    """Return a function that makes PNG bytes."""
    def _make(w, h, *, color=255, alpha=False):
        ch = 4 if alpha else 3
        img = np.ones((h, w, ch), dtype=np.uint8) * color
        ok, buf = cv2.imencode(".png", img)
        assert ok
        return buf.tobytes(), img
    return _make


@pytest.fixture(scope="session")
def small_png_bytes(png_factory):
    b, _ = png_factory(100, 100, color=255)
    return b


@pytest.fixture
def moto_s3():
    with mock_aws():
        import boto3
        s3 = boto3.client("s3", region_name="ap-northeast-1")
        yield s3

@pytest.fixture
def input_output_buckets(moto_s3):
    moto_s3.create_bucket(Bucket="input-bucket", CreateBucketConfiguration={"LocationConstraint": "ap-northeast-1"})
    moto_s3.create_bucket(Bucket="output-bucket", CreateBucketConfiguration={"LocationConstraint": "ap-northeast-1"},)
    return "input-bucket", "output-bucket"


@pytest.fixture(scope="session")
def test_image():
    """Real image for performance testing"""
    image_path = "./tests/fixtures/test_image.png"
    with open(image_path, "rb") as f:
        return f.read()

@pytest.fixture(scope="session")
def small_image(test_image):
    """Original size"""
    return test_image

@pytest.fixture(scope="session")
def large_image(test_image):
    """Large version - Lambda max dims"""
    nparr = np.frombuffer(test_image, np.uint8)
    img = cv2.imdecode(nparr, cv2.IMREAD_UNCHANGED)
    resized = cv2.resize(img, (4096, 4096), interpolation=cv2.INTER_LANCZOS4)
    success, encoded = cv2.imencode('.jpg', resized)
    return encoded.tobytes()

@pytest.fixture
def memory_monitor():
    """Monitor peak memory usage during test execution"""
    peak_mem = [0]
    stop_event = threading.Event()
    
    def monitor():
        process = psutil.Process(os.getpid())
        while not stop_event.is_set():
            peak_mem[0] = max(peak_mem[0], process.memory_info().rss)
            time.sleep(0.01)
    
    monitor_thread = threading.Thread(target=monitor, daemon=True)
    monitor_thread.start()
    
    yield peak_mem
    
    stop_event.set()
    monitor_thread.join()