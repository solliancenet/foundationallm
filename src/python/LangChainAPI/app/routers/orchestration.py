from fastapi import APIRouter, Depends
from app.dependencies import validate_api_key_header

from foundationallm.credentials import AzureCredential
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from foundationallm.langchain.orchestration import OrchestrationManager

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
    Retrieves a completion response from a language model.
    
    Parameters
    ----------
    completion_request : CompletionRequest
        The request object containing the metadata required to build a LangChain agent
        and generate a completion.

    Returns
    -------
    CompletionResponse
        Object containing the completion response and token usage details.
    """
    orchestration_manager = OrchestrationManager(completion_request = completion_request, credential=AzureCredential())
    return orchestration_manager.run(completion_request.user_prompt)