from fastapi import Depends, HTTPException
from fastapi.security import APIKeyHeader
from foundationallm.config import Configuration

class APIKeyValidator:
    
    def __init__(self, api_key_name:str, config: Configuration):
        self.api_key_value = config.get_value(api_key_name)
        self.api_key_header = APIKeyHeader(name='X-API-Key')

    def validate_api_key(self, x_api_key: str = Depends(self.api_key_header)):
        if x_api_key != self.api_key_value:
            raise HTTPException(
                status_code = 401,
                detail = 'Invalid API key. You need to provide a valid API key in the X-API-KEY header.')