from fastapi import Depends, HTTPException
from fastapi.security import APIKeyHeader

from foundationallm.auth import APIKeyValidator
from foundationallm.config import Configuration
   
async def validate_api_key_header(app_config: Configuration, x_api_key: str = Depends(APIKeyHeader(name='X-API-Key'))):
    """
    Validates that the X-API-Key value in the request header matches
    the key expected for this API.
    
    Parameters
    ----------
    - app_config: Configuration
        The configuration object used to retrieve application configuration values.
    - x_api_key : str
        The X-API-Key value in the request header.
    """
    
    result = APIKeyValidator(
        api_key_name='foundationallm-langchain-api-key',
        config=app_config
    ).validate_api_key(x_api_key)
    
    if not result:
        raise HTTPException(
            status_code = 401,
            detail = f'Invalid API key. You must provide a valid API key in the X-API-KEY header.'
        )
