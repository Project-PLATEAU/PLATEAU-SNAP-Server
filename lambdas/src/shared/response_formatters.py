from __future__ import annotations

import json
import logging
from dataclasses import dataclass, asdict
from datetime import datetime, timezone
from typing import Dict


@dataclass(slots=True)
class RespBody:
    status: str
    message: str | None = None
    path: str | None = None
    coordinates: str | None = None
    texture_coordinates: str | None = None


def _resp(code: int, status: str, *, message: str | None = None, path: str | None = None, coordinates: str | None = None,
    texture_coordinates: str | None = None,) -> Dict:

    body = RespBody(status, message, path, coordinates, texture_coordinates)
    body_dict = {k: v for k, v in asdict(body).items() if v is not None}
    return {
        "statusCode": code,
        "headers": {"Content-Type": "application/json; charset=utf-8"},
        "body": json.dumps(body_dict, ensure_ascii=False),
    }


class JsonFormatter(logging.Formatter):
    def format(self, rec):
        return json.dumps(
            {
                "level": rec.levelname,
                "message": rec.getMessage(),
                "time": datetime.now(timezone.utc).isoformat(timespec="seconds").replace("+00:00", "Z"),
            },
            ensure_ascii=False,
        )