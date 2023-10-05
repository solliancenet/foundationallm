from fastapi import FastAPI, Depends, HTTPException
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

source_csv_file_url = secrets_client.get_secret('source-csv-file-url').value

def api_key_auth(x_api_key: str = Depends(api_key_header)):
    if x_api_key != api_key_value:
        raise HTTPException(
            status_code = 401,
            detail = 'Invalid API key. You need to provide a valid API key in the X-API-KEY header.')

app = FastAPI()

@app.get('/')
async def root():
    return { 'message': 'This is the Solliance AI Copilot powered by FoundationaLLM!' }

@app.get('/status', dependencies=[Depends(api_key_auth)])
async def get_status():
    return 'ready'

@app.post('/orchestration/completion', dependencies=[Depends(api_key_auth)])
async def get_completion(prompt: PromptModel):
    
    agent = CSVAgent(source_csv_file_url)
    return agent.run(prompt)

@app.post('/orchestration/summary', dependencies=[Depends(api_key_auth)])
async def get_summary(content: str) -> str:
    pass


if __name__ == '__main__':
    uvicorn.run('main:app', host='0.0.0.0', port=8765, reload=True)