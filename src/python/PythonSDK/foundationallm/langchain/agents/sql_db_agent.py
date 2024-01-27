from langchain.agents import AgentType
from langchain_community.agent_toolkits import create_sql_agent
from langchain_community.callbacks import get_openai_callback

from foundationallm.config import Configuration, Context
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.data_sources.sql import SQLDatabaseConfiguration
from foundationallm.langchain.data_sources.sql import SQLDatabaseFactory
from foundationallm.langchain.language_models import LanguageModelBase
from foundationallm.langchain.toolkits import SecureSQLDatabaseToolkit
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse

class SqlDbAgent(AgentBase):
    """
    Agent for interacting with SQL databases.
    """

    def __init__(
            self,
            completion_request: CompletionRequest,
            llm: LanguageModelBase,
            config: Configuration,
            context: Context):
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
        """
        ds_config = {}
        for ds in completion_request.data_sources:
            ds_config: SQLDatabaseConfiguration = ds.configuration

        self.agent_prompt_prefix = completion_request.agent.prompt_prefix
        self.agent_prompt_suffix = completion_request.agent.prompt_suffix

        self.llm = llm.get_completion_model(completion_request.language_model)
        self.sql_db_config: SQLDatabaseConfiguration = ds_config
        self.context = context

        FORMAT_INSTRUCTIONS = """Use the following format:
Question: the input question you must answer
Thought: you should always think about what to do
Action: the action to take, can only be one of: {tool_names}
Action Input: the input to the action, never add backticks "`" or single quotes "'" around the action input
Observation: the result of the action
(this Thought/Action/Action Input/Observation can repeat N times)
Thought: I now know the final answer
Final Answer: the final answer to the original input question
Please respect the order of the steps Thought/Action/Action Input/Observation
"""

        self.agent = create_sql_agent(
            llm = self.llm,
            toolkit = SecureSQLDatabaseToolkit(
                db = SQLDatabaseFactory(sql_db_config = self.sql_db_config,
                                        config = config).get_sql_database(),
                llm=self.llm,
                reduce_k_below_max_tokens=True,
                username = self.context.get_upn(),
                use_row_level_security = self.sql_db_config.row_level_security_enabled
            ),
            agent_type = AgentType.ZERO_SHOT_REACT_DESCRIPTION,
            verbose = True,
            prefix = self.agent_prompt_prefix,
            format_instructions = FORMAT_INSTRUCTIONS,
            suffix = self.agent_prompt_suffix,
            agent_executor_kwargs={
                'handle_parsing_errors': 'Check your output and make sure it conforms!'
            }
        )

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
            completion = self.agent.invoke({'input':prompt})

            return CompletionResponse(
                completion = completion['output'],
                user_prompt = prompt,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )
