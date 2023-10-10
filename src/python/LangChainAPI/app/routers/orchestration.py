import os
from fastapi import APIRouter, Depends
from app.dependencies import validate_api_key_header
from foundationallm.credentials import AzureCredential
from foundationallm.config import Configuration
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse, SummaryRequest, SummaryResponse
from foundationallm.langchain.openai_models import AzureChatLLM
from foundationallm.langchain.agents import SqlDbAgent, SummaryAgent
from foundationallm.langchain.datasources.sql import SqlDbConfig

# initialize config
credential = AzureCredential().get_credential()     
key_vault_name = Configuration().get_value(key="foundationallm-keyvault-name")  
app_config = Configuration(keyvault_name=key_vault_name, credential=credential)

router = APIRouter(
    prefix='/orchestration',
    tags=['orchestration'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('/completion')
async def get_completion(completion_request: CompletionRequest) -> CompletionResponse:
    # TODO: This should be passed in via the completion request as DataSource metadata.
    sql_db_config = SqlDbConfig(
        dialect='mssql',
        host=f'{os.environ.get("foundationallm-langchain-sqldb-testdb-server-name")}.database.windows.net',
        database=os.environ.get("foundationallm-langchain-sqldb-testdb-database-name"),
        username=os.environ.get("foundationallm-langchain-sqldb-testdb-database-username"),
        password=app_config.get_value('foundationallm-langchain-sqldb-testdb-database-password'),
        include_tables=['DailyPrecipReport', 'HailReport', 'Observer', 'ObserverStatus', 'ObserverType', 'Station'],
        # TODO: This value should be passed in via the completion request as PromptHub metadata.
        prompt_prefix="""You are a helpful agent named Coco designed to interact with a SQL database, which contains hail and precipitation reports.

            Given an input question, first create a syntactically correct {dialect} query to run, then look at the results of the query and return the answer to the input question.
            Unless the user specifies a specific number of examples they wish to obtain, always limit your query to at most {top_k} results using the TOP clause as per MS SQL.
            You can order the results by a relevant column to return the most interesting examples in the database.
            Never query for all the columns from a specific table, only ask for the relevant columns given the question.
            
            The date a report was entered is in the ReportDate column in the DailyPrecipReport and HailReport tables.
            The Station table contains the location of where observations were made.
            The Observer table contains details about the person submitting the report.
            
            You have access to tools for interacting with the database.
            Only use the below tools. Only use the information returned by the below tools to construct your final answer.
            You MUST double check your query before executing it. If you get an error while executing a query, rewrite the query and try again.

            DO NOT make any DML statements (INSERT, UPDATE, DELETE, CREATE, DROP, GRANT, etc.) to the database.

            If the question does not seem related to the database, politely answer with your name and details about the types of questions you can answer."""
    )
    
     # TODO: This needs to be swapped out with SDK calls to an agent factory that will return the appropriate object based the metadata object passed in.
    return SqlDbAgent(completion_request=completion_request, llm=AzureChatLLM(app_config=app_config), app_config=app_config, sql_db_config=sql_db_config).run()
    #return CSVAgent(completion_request=completion_request, llm=AzureChatLLM(app_config=app_config), app_config=app_config).run()

@router.post('/summary')
async def get_summary(summary_request: SummaryRequest) -> SummaryResponse:    
    return SummaryAgent(summary_request=summary_request, llm=AzureChatLLM(app_config=app_config), app_config=app_config).run()