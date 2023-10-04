
from fastapi import FastAPI, Depends, HTTPException
from fastapi.security import APIKeyHeader
from foundationallm.models import Session
import uvicorn

import os
from azure.keyvault.secrets import SecretClient
from azure.identity import DefaultAzureCredential

key_vault_url = os.environ['SOLLIANCEAICOPILOT__LANGCHAINAPI__KEYVAULTURL']
credential = DefaultAzureCredential()
secrets_client = SecretClient(key_vault_url, credential=credential)
api_key_value = secrets_client.get_secret('langchain-api-key').value
api_key_header = APIKeyHeader(name='X-API-Key')

def api_key_auth(x_api_key: str = Depends(api_key_header)):
    if x_api_key != api_key_value:
        raise HTTPException(
            status_code = 401,
            detail = 'Invalid API key. You need to provide a valid API key in the X-API-KEY header.')

app = FastAPI()

@app.get('/')
async def root():
    return { 'message': 'FoundationaLLM Agent Hub API' }

@app.get('/status', dependencies=[Depends(api_key_auth)])
async def status():
    return 'ready'

@app.post('/resolve_request', dependencies=[Depends(api_key_auth)])
async def resolve_request(request: Session):
    return 'resolve_request'

if __name__ == '__main__':
    uvicorn.run('main:app', host='0.0.0.0', port=8765, reload=True)