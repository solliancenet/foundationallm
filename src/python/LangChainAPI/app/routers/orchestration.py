from fastapi import APIRouter, Depends
from app.dependencies import get_config, validate_api_key_header

from foundationallm.credentials import AzureCredential
from foundationallm.models.orchestration import *
from foundationallm.langchain.agents import AgentFactory

# Initialize config
app_config = get_config(credential=AzureCredential())

# Initialize API routing
router = APIRouter(
    prefix='/orchestration',
    tags=['orchestration'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('/completion')
async def get_completion(completion_request: CompletionRequest) -> CompletionResponse:
    completion_agent = AgentFactory(request = completion_request, config = app_config).get_agent()
    return completion_agent.run()
    
@router.post('/summary')
async def get_summary(summary_request: SummaryRequest) -> SummaryResponse:
    summary_agent = AgentFactory(request = summary_request, config = app_config).get_agent()
    return summary_agent.run()