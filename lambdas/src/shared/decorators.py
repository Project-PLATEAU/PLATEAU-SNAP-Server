import json
import logging
from functools import wraps

from .response_formatters import _resp

logger = logging.getLogger()

class ApiError(Exception):
    def __init__(self, status_code, message):
        self.status_code = status_code
        self.message = message
        super().__init__(self.message)

    def __str__(self) -> str:
        return f"{self.status_code}: {self.message}"

def api_handler(func):
    @wraps(func)
    def wrapper(event, context):
        try:
            raw_body = event.get("body", event)
            if isinstance(raw_body, str):
                body = json.loads(raw_body)
            else:
                body = raw_body
            
            return func(body, context)

        except json.JSONDecodeError:
            logger.warning("Request body is not valid JSON.")
            return _resp(400, "error", message="Request body must be valid JSON")
        
        except ApiError as e:
            logger.warning("API Error: %s", e.message)
            return _resp(e.status_code, "error", message=e.message)
            
        except Exception:
            logger.exception("Unhandled internal error")
            return _resp(500, "error", message="An internal processing error occurred")

    return wrapper