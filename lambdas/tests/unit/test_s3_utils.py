import pytest
from botocore.exceptions import ClientError
from src.shared.s3_utils import _parse_s3_uri, _handle_s3_client_error
from src.shared.decorators import ApiError

def test_parse_accepts_variants():
    assert _parse_s3_uri("s3://b/k").netloc == "b"
    assert _parse_s3_uri("b/k").netloc == "b"
    with pytest.raises(ApiError):
        _parse_s3_uri("http://b/k")

def test_handle_maps_errors():
    def mk(code):
        return ClientError({"Error": {"Code": code}}, "GetObject")
    with pytest.raises(Exception) as e404:
        _handle_s3_client_error(mk("NoSuchKey"), "b", "k")
    assert "not found" in str(e404.value)
    with pytest.raises(Exception) as e403:
        _handle_s3_client_error(mk("AccessDenied"), "b", "k")
    assert "Access denied" in str(e403.value)
    with pytest.raises(Exception) as e502:
        _handle_s3_client_error(mk("Weird"), "b", "k")
    assert "502" in str(e502.value)
