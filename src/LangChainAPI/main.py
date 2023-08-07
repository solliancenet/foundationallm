from http.client import HTTPException
from fastapi import FastAPI, Depends
from fastapi.security import APIKeyHeader
import uvicorn

import os
from azure.keyvault.secrets import SecretClient
from azure.identity import DefaultAzureCredential

from solliance.aicopilot import PromptModel
from solliance.aicopilot import CSVAgent

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

@app.get('/', dependencies=[Depends(api_key_auth)])
async def root():
    return { 'message': 'This is the Solliance AI Copilot powered by FoundationalLM!' }

@app.post('/run')
async def run(prompt: PromptModel):
    
    agent = CSVAgent()
    return agent.run(prompt)



if __name__ == '__main__':
    uvicorn.run('main:app', host='0.0.0.0', port=8765, reload=True)