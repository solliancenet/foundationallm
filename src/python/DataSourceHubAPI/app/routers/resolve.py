from fastapi import APIRouter, Depends
from app.dependencies import validate_api_key_header
from foundationallm.hubs.data_source import DataSourceHubRequest, DataSourceHubResponse, DataSourceHub

router = APIRouter(
    prefix='/resolve',
    tags=['resolve'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('/')
async def resolve(request:DataSourceHubRequest) -> DataSourceHubResponse:    
    return DataSourceHub().resolve(request)