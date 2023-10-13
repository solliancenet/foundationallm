from fastapi import APIRouter, Depends
from app.dependencies import validate_api_key_header
from foundationallm.hubs.prompt import PromptHubRequest, PromptHubResponse, PromptHub

router = APIRouter(
    prefix='/resolve_request',
    tags=['resolve_request'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('/')
async def resolve_request(request: PromptHubRequest) -> PromptHubResponse:
    return PromptHub().resolve_request(request)