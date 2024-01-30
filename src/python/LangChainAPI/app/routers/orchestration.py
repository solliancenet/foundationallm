"""
The API endpoint for returning the completion from the LLM for the specified user prompt.
"""
from typing import Optional
from fastapi import APIRouter, Depends, Header, Request, Body
from foundationallm.config import Context
from foundationallm.models.orchestration import (
    CompletionRequestBase,
    CompletionRequest,
    KnowledgeManagementCompletionRequest,
    CompletionResponse
)
from foundationallm.langchain.orchestration import OrchestrationManager
from app.dependencies import handle_exception, validate_api_key_header

# Initialize API routing
router = APIRouter(
    prefix='/orchestration',
    tags=['orchestration'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

# temporary to support legacy agents alongside the knowledge-management agent
async def resolve_completion_request(request_body: dict = Body(...)) -> CompletionRequestBase:  
    agent_type = request_body.get("agent", {}).get("type", None)  
    if agent_type == "knowledge-management":  
        return KnowledgeManagementCompletionRequest(**request_body)  
    else:  
        return CompletionRequest(**request_body)

@router.post('/completion')
async def get_completion(
    request : Request,
    completion_request: CompletionRequestBase = Depends(resolve_completion_request),    
    x_user_identity: Optional[str] = Header(None)) -> CompletionResponse:
    """
    Retrieves a completion response from a language model.
    
    Parameters
    ----------
    completion_request : CompletionRequest
        The request object containing the metadata required to build a LangChain agent
        and generate a completion.
    request : Request
        The underlying HTTP request.
    x_user_identity : str
        The optional X-USER-IDENTITY header value.

    Returns
    -------
    CompletionResponse
        Object containing the completion response and token usage details.
    """
    try:
        orchestration_manager = OrchestrationManager(completion_request = completion_request,
                                                     configuration=request.app.extra['config'],
                                                     context=Context(user_identity=x_user_identity))
        return orchestration_manager.run(completion_request.user_prompt)
    except Exception as e:
        handle_exception(e)
