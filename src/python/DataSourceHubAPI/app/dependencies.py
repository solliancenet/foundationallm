from typing import Annotated
from fastapi import Depends, HTTPException
from fastapi.security import APIKeyHeader
from foundationallm.credentials import AzureCredential
from foundationallm.config import Configuration

def get_config(credential: Annotated[AzureCredential, Depends(AzureCredential)]):
    """
    Gets a configuration object for retrieving application configuration values.
    """
    keyvault_name = Configuration().get_value(key='foundationallm-keyvault-name')
    return Configuration(keyvault_name=keyvault_name, credential=credential)

async def validate_api_key_header(app_config: Annotated[Configuration, Depends(get_config)], x_api_key: str = Depends(APIKeyHeader(name='X-API-Key'))):
    """
    Validates that the X-API-Key value in the request header matches
    the key expected for this API.
    
    Parameters
    ----------
    - app_config: Configuration
        Used for retrieving application configuration settings.
    - x_api_key : str
        The X-API-Key value in the request header.
    """

    result = x_api_key == app_config.get_value('foundationallm-datasourcehub-api-key')
    
    if not result:
        raise HTTPException(
            status_code = 401,
            detail = f'Invalid API key. You must provide a valid API key in the X-API-KEY header.'
        )