import os
from langchain.agents import AgentExecutor, create_sql_agent
from langchain.agents.agent_toolkits import SQLDatabaseToolkit
from langchain.agents.agent_types import AgentType
from foundationallm.langchain.datasources.sql import SqlDbConfig, MsSqlServer
from foundationallm.langchain.openai_models import AzureChatLLM
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from foundationallm.config import Configuration
#from langchain.prompts import PromptTemplate

class SqlDbAgent():
    """
    Agent for interacting with SQL databases.
    """

    def __init__(self, content: CompletionRequest, llm: AzureChatLLM, config: Configuration):
        self.user_prompt = content.prompt
        self.llm = llm.get_chat_model()
        self.config = config
        
        # TODO: This should be passed in.
        self.sql_db_config = SqlDbConfig(
            dialect='mssql',
            host=f'{os.environ.get("SQL_DB_SERVER_NAME")}.database.windows.net',
            database=os.environ.get("SQL_DB_DATABASE_NAME"),
            username=os.environ.get("SQL_DB_USERNAME"),
            password=config.get_value('sql-db-password'),
            include_tables=['DailyPrecipReport', 'HailReport', 'Observer', 'ObserverStatus', 'ObserverType', 'Station'],
            # TODO: Work out how to break this up, so the standard language doesn't have be to included here, but still gets used.
            prompt="""
                You are an agent named Coco designed to interact with a SQL database.
                You help users answer their questions about hail and precipitation data.
                You are working with a SQL database that contains hail and precipitation reports entered by citizen observers.
                The date a report was entered is in the ReportDate column in the DailyPrecipReport and HailReport tables.
                The Station table contains the location of where observations were made.
                The Observer table contains details about the person submitting the report.

                Given an input question, create a syntactically correct mssql query to run, then look at the results of the query and return the answer.
                Unless the user specifies a specific number of examples they wish to obtain, always limit your query to at most 10 results.
                You can order the results by a relevant column to return the most interesting examples in the database.
                Never query for all the columns from a specific table, only ask for the relevant columns given the question.
                You have access to tools for interacting with the database.
                Only use the below tools. Only use the information returned by the below tools to construct your final answer.
                You MUST double check your query before executing it. If you get an error while executing a query, rewrite the query and try again.

                DO NOT make any DML statements (INSERT, UPDATE, DELETE, DROP etc.) to the database.

                If the question does not seem related to the database, politely answer with your name and details about the types of questions you can answer."""
        )
        
        self.agent = create_sql_agent(
            llm = self.llm,
            toolkit = SQLDatabaseToolkit( #TODO: Swap out with overriden, secure toolkit.
                db=MsSqlServer(self.sql_db_config).get_database(),
                llm=self.llm,
                reduce_k_below_max_tokens=True
            ),
            agent_type = AgentType.ZERO_SHOT_REACT_DESCRIPTION,
            verbose = True,
            prefix = self.sql_db_config.prompt,
            agent_executor_kwargs={
                'handle_parsing_errors': True
            }
        )
        
    def run(self) -> CompletionResponse:
        return CompletionResponse(
            completion = self.agent.run(self.user_prompt),
            user_prompt= self.user_prompt
        )