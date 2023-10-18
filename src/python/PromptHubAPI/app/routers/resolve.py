from fastapi import APIRouter, Depends
from app.dependencies import validate_api_key_header
from foundationallm.hubs.prompt import PromptHubRequest, PromptHubResponse, PromptHub

router = APIRouter(
    prefix='/resolve',
    tags=['resolve'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('/')
async def resolve(request: PromptHubRequest) -> PromptHubResponse:
    return PromptHub().resolve(request)