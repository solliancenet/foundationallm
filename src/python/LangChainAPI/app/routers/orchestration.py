from fastapi import APIRouter, Depends
from app.dependencies import validate_api_key_header
from foundationallm.config import Configuration
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse, SummaryRequest, SummaryResponse
from foundationallm.langchain.openai_models import AzureChatLLM
from foundationallm.langchain.agents import SqlDbAgent, SummaryAgent

router = APIRouter(
    prefix='/orchestration',
    tags=['orchestration'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('/completion')
async def get_completion(content: CompletionRequest) -> CompletionResponse:
    config=Configuration()
    llm = AzureChatLLM(
        base_url='https://foundationallm-01.openai.azure.com/',
        deployment_name='completions',
        config=config
    )
     # TODO: This needs to be swapped out with SDK calls to an agent factory that will return the appropriate object based the metadata object passed in.
    return SqlDbAgent(content=content, llm=llm, config=config).run()
    #agent = CSVAgent(source_csv_file_url)

@router.post('/summary')
async def get_summary(content: SummaryRequest) -> SummaryResponse:
    config=Configuration()
    llm = AzureChatLLM(
        base_url='https://foundationallm-01.openai.azure.com/',
        deployment_name='completions',
        config=config
    )
    
    return SummaryAgent(content=content, llm=llm).run()