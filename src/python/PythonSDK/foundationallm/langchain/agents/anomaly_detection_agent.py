from langchain.agents import create_sql_agent, initialize_agent, Tool
from langchain.agents.agent_toolkits import SQLDatabaseToolkit
from langchain.agents.agent_types import AgentType
from langchain.callbacks import get_openai_callback
from langchain.prompts import PromptTemplate
#from langchain.document_loaders import DataFrameLoader

from foundationallm.config import Configuration
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.data_sources.sql import SqlDbConfig
from foundationallm.langchain.data_sources.sql import SqlDbFactory
from foundationallm.langchain.language_models.chat_models import AzureChatOpenAILanguageModel
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse

class AnomalyDetectionAgent(AgentBase):
    """
    Agent for performing anomaly detection.
    """
    
    # TODO: This is all very hardcoded for demo purposes.

    def __init__(self, completion_request: CompletionRequest, llm: AzureChatOpenAILanguageModel, app_config: Configuration):
        self.agent_prompt_prefix = completion_request.agent.prompt_template #PromptTemplate.from_template(completion_request.agent.prompt_template)
        self.user_prompt = completion_request.user_prompt
        self.llm = llm.get_language_model()
        # Currently set up to use a SQL Database table as the source system.
        self.sql_db_config: SqlDbConfig = completion_request.data_source.configuration
        
        self.sql_agent_prompt = """
        You are an anomaly detection agent designed to interact with a SQL database.
        
        Given an input question, first create a syntactically correct {dialect} query to run, then look at the results of the query and return the answer to the input question. Unless the user specifies a specific number of examples they wish to obtain, always limit your query to at most {top_k} results using the TOP clause as per MS SQL. You can order the results by a relevant column to return the most interesting examples in the database. Never query for all the columns from a specific table, only ask for the relevant columns given the question.
        
        You have access to tools for interacting with the database. Only use the below tools. Only use the information returned by the below tools to construct your final answer. You MUST double check your query before executing it. If you get an error while executing a query, rewrite the query and try again.
        
        DO NOT make any DML statements (INSERT, UPDATE, DELETE, CREATE, DROP, GRANT, etc.) to the database.
        
        If the question does not seem related to the database, politely answer with your name and details about the types of questions you can answer.
        """
        
        self.sql_agent = create_sql_agent(
            llm = self.llm,
            toolkit = SQLDatabaseToolkit(
                db = SqlDbFactory(sql_db_config = self.sql_db_config, app_config = app_config).get_sql_database(),
                llm=self.llm,
                reduce_k_below_max_tokens=True
            ),
            agent_type = AgentType.ZERO_SHOT_REACT_DESCRIPTION,
            verbose = True,
            prefix = PromptTemplate.from_template(self.sql_agent_prompt),
            agent_executor_kwargs={
                'handle_parsing_errors': True
            }
        )
        
        self.tools = [
            Tool(
                name = 'Rum Database',
                func = self.sql_agent.run,
                description = 'Useful for answering questions about anomalies in rum bottle volumes and prices.'
            )
        ]
        
        self.agent = initialize_agent(
            tools = self.tools,
            llm = self.llm,
            agent = AgentType.ZERO_SHOT_REACT_DESCRIPTION,
            verbose = True,
            agent_kwargs={
                'prefix': self.agent_prompt_prefix
            }
        )
        
    @property
    def prompt_template(self) -> str:
        return self.agent.agent.llm_chain.prompt.template
        
    def run(self) -> CompletionResponse:
        with get_openai_callback() as cb:
            return CompletionResponse(
                completion = self.agent.run(self.user_prompt),
                user_prompt= self.user_prompt,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )