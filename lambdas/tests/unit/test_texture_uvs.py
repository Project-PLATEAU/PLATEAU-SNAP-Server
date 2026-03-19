import numpy as np
from src.texture_building.handler import crop_and_generate_uvs

def test_uv_coordinates_are_normalized():
    img = np.ones((100,100,3), np.uint8)*255
    wkt = "POLYGON((10 10, 60 10, 60 60, 10 60, 10 10))"
    crop, uv_wkt = crop_and_generate_uvs(img, wkt)
    assert crop.shape[:2] == (50,50)
    assert "POLYGON ((" in uv_wkt
    # Endpoints should be near (0,1) and (1,0)
    assert "(0 1" in uv_wkt or "(0 1)" in uv_wkt
