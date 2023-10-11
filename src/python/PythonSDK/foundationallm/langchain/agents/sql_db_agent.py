from langchain.agents import create_sql_agent
from langchain.agents.agent_toolkits import SQLDatabaseToolkit
from langchain.agents.agent_types import AgentType
from langchain.callbacks import get_openai_callback
from langchain.prompts import PromptTemplate

from foundationallm.config import Configuration
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.data_sources.sql import SqlDbConfig
from foundationallm.langchain.data_sources.sql import SqlDbFactory
from foundationallm.langchain.language_models.chat_models import AzureChatOpenAILanguageModel
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse

class SqlDbAgent(AgentBase):
    """
    Agent for interacting with SQL databases.
    """

    def __init__(self, completion_request: CompletionRequest, llm: AzureChatOpenAILanguageModel, app_config: Configuration):
        self.agent_prompt_prefix = PromptTemplate.from_template(completion_request.agent.prompt_template)
        self.user_prompt = completion_request.user_prompt
        self.llm = llm.get_language_model()
        self.sql_db_config: SqlDbConfig = completion_request.data_source.configuration

        self.agent = create_sql_agent(
            llm = self.llm,
            toolkit = SQLDatabaseToolkit( #TODO: Swap out with overridden, secure toolkit.
                db = SqlDbFactory(sql_db_config = self.sql_db_config, app_config = app_config).get_sql_database(),
                llm=self.llm,
                reduce_k_below_max_tokens=True
            ),
            agent_type = AgentType.ZERO_SHOT_REACT_DESCRIPTION,
            verbose = True,
            prefix = self.agent_prompt_prefix,
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