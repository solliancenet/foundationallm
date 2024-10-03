"""
Main entry-point for the FoundationaLLM DataSourceHubAPI.
Runs web server exposing the API.
"""
from fastapi import FastAPI
from app.dependencies import API_NAME, get_config
from app.routers import (
    manage,
    status
)
from foundationallm.telemetry import Telemetry

# Open a connection to the app configuration
config = get_config()
# Start collecting telemetry
# Telemetry.configure_monitoring(config, f'FoundationaLLM:APIEndpoints:{API_NAME}:Essentials:AppInsightsConnectionString', API_NAME)

app = FastAPI(
    title=f'FoundationaLLM {API_NAME}',
    summary='API for retrieving DataSource metadata',
    description=f"""The FoundationaLLM {API_NAME} is a wrapper around DataSourceHub
                functionality contained in the foundationallm Python SDK.""",
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
