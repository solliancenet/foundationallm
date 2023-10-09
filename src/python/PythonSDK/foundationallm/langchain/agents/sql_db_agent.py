from langchain.agents import AgentExecutor, create_sql_agent
from langchain.agents.agent_toolkits import SQLDatabaseToolkit
from langchain.agents.agent_types import AgentType
from langchain.callbacks import get_openai_callback
from langchain.prompts import PromptTemplate

from foundationallm.langchain.datasources.sql import SqlDbConfig, MsSqlServer
from foundationallm.langchain.openai_models import AzureChatLLM
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from foundationallm.config import Configuration

class SqlDbAgent():
    """
    Agent for interacting with SQL databases.
    """

    def __init__(self, completion_request: CompletionRequest, llm: AzureChatLLM, app_config: Configuration, sql_db_config: SqlDbConfig):
        self.user_prompt = completion_request.user_prompt
        self.llm = llm.get_chat_model()
        self.sql_db_config = sql_db_config
        
        self.agent = create_sql_agent(
            llm = self.llm,
            toolkit = SQLDatabaseToolkit( #TODO: Swap out with overriden, secure toolkit.
                db=MsSqlServer(self.sql_db_config).get_database(),
                llm=self.llm,
                reduce_k_below_max_tokens=True
            ),
            agent_type = AgentType.ZERO_SHOT_REACT_DESCRIPTION,
            verbose = True,
            prefix = PromptTemplate.from_template(self.sql_db_config.prompt_prefix),
            agent_executor_kwargs={
                'handle_parsing_errors': True
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