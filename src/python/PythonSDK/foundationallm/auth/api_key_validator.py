from fastapi import Depends, HTTPException
from fastapi.security import APIKeyHeader

class ApiKeyValidator:
    
    def __init__(self, api_key_value: str):
        self.api_key_value = api_key_value
        self.api_key_header = APIKeyHeader(name='X-API-Key')

    def api_key_auth(self, x_api_key: str = Depends(self.api_key_header)):
        if x_api_key != self.api_key_value:
            raise HTTPException(
                status_code = 401,
                detail = 'Invalid API key. You need to provide a valid API key in the X-API-KEY header.')