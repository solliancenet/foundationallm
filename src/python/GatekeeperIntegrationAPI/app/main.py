"""
Main entry-point for the FoundationaLLM DataSourceHubAPI.
Runs web server exposing the API.
"""
from fastapi import FastAPI
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor
from app.dependencies import API_NAME, get_config
from app.routers import (
    analyze,
    manage,
    status
)

app = FastAPI(
    title=f'FoundationaLLM {API_NAME}',
    summary='API for extending the FoundationaLLM GatekeeperAPI',
    description=f"""The FoundationaLLM {API_NAME} is a service used to extend the
            FoundationaLLM GatekeeperAPI with extra capabilities""",
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
        'name': 'FoundationaLLM Software License',
        'url': 'https://www.foundationallm.ai/license',
    },
    config=get_config()
)

FastAPIInstrumentor.instrument_app(app)

app.include_router(analyze.router)
app.include_router(manage.router)
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
    return { 'message': f'FoundationaLLM {API_NAME}' }
