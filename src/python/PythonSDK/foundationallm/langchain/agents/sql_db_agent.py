from langchain.agents import create_sql_agent
from langchain.agents.agent_types import AgentType
from langchain.callbacks import get_openai_callback

from foundationallm.config import Configuration, Context
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.data_sources.sql import SQLDatabaseConfiguration
from foundationallm.langchain.data_sources.sql import SQLDatabaseFactory
from foundationallm.langchain.language_models import LanguageModelBase
from foundationallm.langchain.toolkits import SecureSQLDatabaseToolkit
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from foundationallm.langchain.parsers import FLLMOutputParser

class SqlDbAgent(AgentBase):
    """
    Agent for interacting with SQL databases.
    """

    def __init__(self, completion_request: CompletionRequest, llm: LanguageModelBase,
                 config: Configuration, context: Context, is_testing: bool = False):
        """
        Initializes a SQL database agent.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent and data source metadata.
        llm : LanguageModelBase
            The language model to use for executing the completion request.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        context : Context
            User context under which to run the completion request.
        is_testing : bool
            Raises the iteration limit and enables a custom output parser for testing purposes
        """
        self.agent_prompt_prefix = completion_request.agent.prompt_prefix
        self.agent_prompt_suffix = completion_request.agent.prompt_suffix

        self.llm = llm.get_completion_model(completion_request.language_model)
        self.sql_db_config: SQLDatabaseConfiguration = completion_request.data_source.configuration
        self.context = context

        create_sql_agent_args = {
            'llm': self.llm,
            'toolkit': SecureSQLDatabaseToolkit(
                db = SQLDatabaseFactory(sql_db_config = self.sql_db_config,
                                        config = config).get_sql_database(),
                llm=self.llm,
                reduce_k_below_max_tokens=True,
                username = self.context.get_upn(),
                use_row_level_security = self.sql_db_config.row_level_security_enabled
            ),
            'agent_type': AgentType.ZERO_SHOT_REACT_DESCRIPTION,
            'verbose': True,
            'prefix': self.agent_prompt_prefix,
            'suffix': self.agent_prompt_suffix,
            'agent_executor_kwargs': {
                'handle_parsing_errors': 'Check your output and make sure it conforms!'
            }
        }

        if is_testing:
            create_sql_agent_args['max_iterations'] = 15

        self.agent = create_sql_agent(**create_sql_agent_args)

        if is_testing:
            self.agent.agent.output_parser = FLLMOutputParser()

    @property
    def prompt_template(self) -> str:
        """
        Property for viewing the agent's prompt template.
        
        Returns
        str
            Returns the prompt template for the agent.
        """
        return self.agent.agent.llm_chain.prompt.template

    def run(self, prompt: str) -> CompletionResponse:
        """
        Executes a query against a SQL database.

        Parameters
        ----------
        prompt : str
            The prompt for which a completion is begin generated.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the database query completion response, 
            the user_prompt, and token utilization and execution cost details.
        """
        with get_openai_callback() as cb:
            return CompletionResponse(
                completion = self.agent.run(prompt),
                user_prompt= prompt,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )
