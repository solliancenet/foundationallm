import uvicorn
from fastapi import FastAPI
from app.routers import orchestration, status

app = FastAPI(
    title='FoundationaLLM LangChainAPI',
    summary='API for interacting with language models using LangChain.',
    description='The FoundationaLLM LangChainAPI is a wrapper around LangChain functionality contained in the foundationallm.core Python SDK.',
    version='1.0.0',
    contact={
        'name':'Solliance, Inc.',
        'email':'contact@solliance.net',
        'url':'https://solliance.net/' 
    },
    openapi_url='/swagger/v1/swagger.json',
    docs_url='/swagger',
    redoc_url=None,
    license_info={
        "name": "FoundationaLLM Software License",
        "url": "https://www.foundationallm.ai/license",
    }
)

app.include_router(orchestration.router)
app.include_router(status.router)

@app.get('/')
async def root():
    """
    Root path of the API.
    
    Returns
    -------
    str
        Returns a JSON object containing a message and value.
    """
    return { 'message': 'This is the Solliance AI Copilot powered by FoundationaLLM!' }

if __name__ == '__main__':
    uvicorn.run('app.main:app', host='0.0.0.0', port=8765, reload=True)