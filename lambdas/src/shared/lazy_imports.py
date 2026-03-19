from __future__ import annotations

import importlib
from typing import Any

_CACHE: dict[str, Any] = {}


def lazy_import(module: str) -> Any:
    mod = _CACHE.get(module)
    if mod is None:
        mod = importlib.import_module(module)
        _CACHE[module] = mod
    return mod


def get_numpy() -> Any:
    return lazy_import("numpy")


def get_cv2() -> Any:
    return lazy_import("cv2")


def get_mercantile() -> Any:
    return lazy_import("mercantile")


def get_shapely_wkt() -> Any:
    return lazy_import("shapely.wkt")


def get_shapely_polygon() -> Any:
    # returns the Polygon class
    geom = lazy_import("shapely.geometry")
    return geom.Polygon
