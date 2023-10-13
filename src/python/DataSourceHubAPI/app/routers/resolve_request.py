from fastapi import APIRouter, Depends
from app.dependencies import validate_api_key_header
from foundationallm.hubs.data_source import DataSourceHubRequest, DataSourceHubResponse, DataSourceHub

router = APIRouter(
    prefix='/resolve_request',
    tags=['resolve_request'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('/')
async def resolve_request(request:DataSourceHubRequest) -> DataSourceHubResponse:    
    return DataSourceHub().resolve_request(request)