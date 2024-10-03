"""
Provides dependencies for API calls.
"""
import logging
import time
from fastapi import Depends, HTTPException
from fastapi.security import APIKeyHeader
from foundationallm.config import Configuration
from foundationallm.telemetry import Telemetry

# Initialize telemetry logging
logger = None
tracer = None

__config: Configuration = None
API_NAME = 'LangChainAPI'

def get_config(action: str = None) -> Configuration:
    """
    Obtains the application configuration settings.

    Returns
    -------
    Configuration
        Returns the application configuration settings.
    """
    global __config
    global logger

    start = time.time()
    if action is not None and action=='refresh':
        __config = Configuration()
    else:
        __config = __config or Configuration()
    end = time.time()

    logger = Telemetry.get_logger(__name__)
    logger.info(f'Time to load config: {end-start}')
    return __config

async def validate_api_key_header(x_api_key: str = Depends(APIKeyHeader(name='X-API-Key'))) -> bool:
    """
    Validates that the X-API-Key value in the request header matches the key expected for this API.

    Parameters
    ----------
    x_api_key : str
        The X-API-Key value in the request header.

    Returns
    bool
        Returns True of the X-API-Key value from the request header matches the expected value.
        Otherwise, returns False.
    """

    result = x_api_key == get_config().get_value(f'FoundationaLLM:APIEndpoints:{API_NAME}:Essentials:APIKey')

    if not result:
        logger = Telemetry.get_logger(__name__)
        logger.error('Invalid API key. You must provide a valid API key in the X-API-KEY header.')
        raise HTTPException(
            status_code = 401,
            detail = 'Invalid API key. You must provide a valid API key in the X-API-KEY header.'
        )

def handle_exception(exception: Exception, status_code: int = 500):
    """
    Handles an exception that occurred while processing a request.

    Parameters
    ----------
    exception : Exception
        The exception that occurred.
    """
    logger = Telemetry.get_logger(__name__)
    logger.error(exception, stack_info=True, exc_info=True)
    raise HTTPException(
        status_code = status_code,
        detail = str(exception)
    ) from exception
