from fastapi import APIRouter, Depends
from app.dependencies import validate_api_key_header

router = APIRouter(
    prefix='/status',
    tags=['status'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.get('/')
async def get_status() -> str:
    """
    Retrieves the status of the API.
    
    Returns
    -------
    string
        String containing the current status of the API.
    """
    
    return 'ready'