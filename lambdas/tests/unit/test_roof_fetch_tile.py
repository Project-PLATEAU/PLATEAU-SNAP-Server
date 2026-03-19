import requests_mock
from src.roof_extraction.handler import fetch_tile, TILE_SIZE, _build_session

def test_fetch_tile_handles_corrupt_and_resizes():
    s = _build_session()
    with requests_mock.Mocker() as m:
        m.get(requests_mock.ANY, content=b"not a png")
        tile = fetch_tile(s, 18, 1, 1)
        assert tile.shape == (TILE_SIZE, TILE_SIZE, 3)
