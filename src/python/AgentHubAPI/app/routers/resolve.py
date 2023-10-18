from fastapi import APIRouter, Depends
from app.dependencies import validate_api_key_header
from foundationallm.hubs.agent import AgentHub, AgentHubRequest, AgentHubResponse

router = APIRouter(
    prefix='/resolve',
    tags=['resolve'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}},
    redirect_slashes=False
)

@router.post('')
async def resolve(request: AgentHubRequest) -> AgentHubResponse:    
    return AgentHub().resolve(request)