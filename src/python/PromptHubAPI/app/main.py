from fastapi import Depends, FastAPI
import uvicorn
from app.routers import resolve_request, status

app = FastAPI(
    title='FoundationaLLM PromptHubAPI',
    summary='API for retrieving Prompt metadata',
    description='The FoundationaLLM PromptHubAPI is a wrapper around PromptHub functionality contained in the foundationallm.core Python SDK.',
    version='0.1.0',
    openapi_url='/swagger/v1/swagger.json',
    docs_url='/swagger',
    redoc_url=None,
    license_info={
        "name": "FoundationaLLM Software License",
        "url": "https://www.foundationallm.ai/license",
    }
)

app.include_router(resolve_request.router)
app.include_router(status.router)

@app.get('/')
async def root():
    return { 'message': 'FoundationaLLM PromptHubAPI' }

if __name__ == '__main__':
    uvicorn.run('main:app', host='0.0.0.0', port=8642, reload=True)