from typing import Annotated
from fastapi import Depends, HTTPException
from fastapi.security import APIKeyHeader
from foundationallm.credentials import AzureCredential
from foundationallm.auth import APIKeyValidator
from foundationallm.config import Configuration
   
async def validate_api_key_header(x_api_key: str = Depends(APIKeyHeader(name='X-API-Key'))):
    """
    Validates that the X-API-Key value in the request header matches
    the key expected for this API.
    
    Parameters
    ----------    
    - x_api_key : str
        The X-API-Key value in the request header.
    """
    
    credential = AzureCredential().get_credential()     
    key_vault_name = Configuration().get_value(key="foundationallm-keyvault-name")  
    app_config = Configuration(keyvault_name=key_vault_name, credential=credential)
    
    result = APIKeyValidator(
        api_key_name='foundationallm-agenthub-api-key',
        config=app_config
    ).validate_api_key(x_api_key)
    
    if not result:
        raise HTTPException(
            status_code = 401,
            detail = f'Invalid API key. You must provide a valid API key in the X-API-KEY header.'
        )
