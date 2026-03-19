import logging
from os import environ
from .response_formatters import JsonFormatter

def get_logger(name: str = None) -> logging.Logger:
    """Get configured logger instance."""
    logger = logging.getLogger(name)
    
    if not logger.handlers:
        handler = logging.StreamHandler()
        handler.setFormatter(JsonFormatter())
        logger.addHandler(handler)
        
        log_level = environ.get("LOG_LEVEL", "INFO").upper()
        logger.setLevel(log_level)
    
    return logger