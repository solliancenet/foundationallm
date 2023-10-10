from fastapi import Depends, FastAPI
import uvicorn
from app.routers import orchestration, status

# FastAPI metadata info: https://fastapi.tiangolo.com/tutorial/metadata/
app = FastAPI(
    title='FoundationaLLM LangChainAPI',
    summary='API for interacting with language models using LangChain.',
    description='The FoundationaLLM LangChainAPI is a wrapper around LangChain functionality contained in the foundationallm.core Python SDK.',
    version='0.1.0', # TODO: Figure out how to make this configurable
    openapi_url='/swagger/v1/swagger.json',
    docs_url='/swagger',
    redoc_url=None
)

app.include_router(orchestration.router)
app.include_router(status.router)

@app.get('/')
async def root():
    return { 'message': 'This is the Solliance AI Copilot powered by FoundationaLLM!' }

if __name__ == '__main__':
    uvicorn.run('app.main:app', host='0.0.0.0', port=8765, reload=True)