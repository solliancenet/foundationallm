
from fastapi import FastAPI, Depends, HTTPException
from fastapi.security import APIKeyHeader
from foundationallm.models import Session
from foundationallm.hubs.agent import AgentHub
# from foundationallm.config import APIKeyValidator
import uvicorn

# validator = APIKeyValidator("agent-hub-api-key")

app = FastAPI()

@app.get('/')
async def root():
    return { 'message': 'FoundationaLLM Agent Hub API' }

@app.get('/status') #, dependencies=[Depends(validator.api_key_auth)])
async def status():
    return 'ready'

@app.post('/resolve_request') #, dependencies=[Depends(validator.api_key_auth)])
async def resolve_request(request: Session):
    ag = AgentHub().resolve_request(request)
    return ag #AgentHub().resolve_request(request)

if __name__ == '__main__':
    uvicorn.run('main:app', host='0.0.0.0', port=8742, reload=True)