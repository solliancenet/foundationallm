from fastapi import APIRouter, Depends
from app.dependencies import validate_api_key_header
from foundationallm.config import Configuration
from foundationallm.models import Prompt
from foundationallm.langchain.openai_models import AzureChat
from foundationallm.langchain.agents import SummaryAgent

router = APIRouter(
    prefix='/orchestration',
    tags=['orchestration'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('/completion')
async def get_completion(prompt: Prompt):
    return 'completion'
    #agent = CSVAgent(source_csv_file_url) # TODO: This needs to be swapped out with SDK calls to construct an agent from the metadata object passed in.
    #return agent.run(prompt)

@router.post('/summary')
async def get_summary(content: str) -> str:
    two_word_prompt_template = """Write a concise two word summary of the following:
    "{text}"
    CONCISE SUMMARY IN TWO WORDS:"""

    llm = AzureChat(
        openai_api_type='azure',
        base_url='https://foundationallm-01.openai.azure.com/',
        deployment_name='completions',
        config=Configuration()
    )
    return SummaryAgent(prompt_template=two_word_prompt_template, llm=llm).run(text=content)