from fastapi import APIRouter, Depends
from app.dependencies import validate_api_key_header

from foundationallm.credentials import AzureCredential
from foundationallm.models.orchestration import *
from foundationallm import SDKClient

# Initialize API routing
router = APIRouter(
    prefix='/orchestration',
    tags=['orchestration'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('/completion')
async def get_completion(completion_request: CompletionRequest) -> CompletionResponse:
    """
    Retrieves a completion from a language model
    
    Parameters
    ----------
    completion_request : CompletionRequest
        The request object containing data required to generate a completion.

    Returns
    -------
    CompletionResponse
        Object containing the completion and token usage details
    """
    # Initialize an SDK client.
    client = SDKClient(credential=AzureCredential())
    
    # Create an agent
    agent = client.create_agent(completion_request = completion_request)
    return agent.run(completion_request.user_prompt)