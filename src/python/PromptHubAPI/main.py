from fastapi import FastAPI, Depends, HTTPException
from fastapi.security import APIKeyHeader
from foundationallm.models import Session
from foundationallm.hubs.prompt import PromptHub
# from foundationallm.config import APIKeyValidator
import uvicorn

# validator = APIKeyValidator("prompt-hub-api-key")

app = FastAPI()

@app.get('/')
async def root():
    return { 'message': 'FoundationaLLM Prompt Hub API' }

@app.get('/status') #, dependencies=[Depends(validator.api_key_auth)])
async def status():
    return 'ready'

@app.post('/resolve_request') #, dependencies=[Depends(validator.api_key_auth)])
async def resolve_request(request: Session):    
    return PromptHub().resolve_request(request)

if __name__ == '__main__':
    uvicorn.run('main:app', host='0.0.0.0', port=8642, reload=True)
