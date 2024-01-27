import pandas as pd
from sqlalchemy import create_engine

from langchain.agents import AgentType, initialize_agent
from langchain_community.agent_toolkits import create_sql_agent
from langchain_community.callbacks import get_openai_callback
from langchain_core.exceptions import OutputParserException
from langchain_core.prompts import PromptTemplate
from langchain_core.tools import Tool
from langchain_experimental.agents.agent_toolkits import create_pandas_dataframe_agent, create_python_agent
from langchain_experimental.tools.python.tool import PythonREPLTool

from foundationallm.config import Configuration
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.language_models import LanguageModelBase
from foundationallm.langchain.data_sources.sql import SQLDatabaseFactory
from foundationallm.langchain.toolkits import SecureSQLDatabaseToolkit
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from foundationallm.langchain.data_sources.sql import SQLDatabaseConfiguration
from foundationallm.langchain.data_sources.sql.mssql import MicrosoftSQLServer
from foundationallm.langchain.toolkits import AnomalyDetectionToolkit

class AnomalyDetectionAgent(AgentBase):
    """
    Agent for performing anomaly detection.
    """

    def __init__(self, completion_request: CompletionRequest, llm: LanguageModelBase, config: Configuration):
        """
        Initializes a anomaly detection agent.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent and data source metadata.
        llm : LanguageModelBase
            The language model to use for executing the completion request.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        # Currently set up to use a SQL Database table as the source system.
        self.sql_db_config = {}
        for ds in completion_request.data_sources:
            self.sql_db_config: SQLDatabaseConfiguration = ds.configuration

        self.agent_prompt_prefix = completion_request.agent.prompt_prefix
        self.llm = llm.get_completion_model(completion_request.language_model)
                
        self.sql_agent_prompt = """You are an anomaly detection agent designed to interact with a SQL database. Given an input question, first create a syntactically correct {dialect} query to run, then look at the results of the query and return the answer to the input question.
        Unless the user specifies a specific number of examples they wish to obtain, always limit your query to at most {top_k} results using the TOP clause as per MS SQL. You can order the results by a relevant column to return the most interesting examples in the database.
        Never query for all the columns from a specific table, only ask for the relevant columns given the question.
        
        DO NOT make any DML statements (INSERT, UPDATE, DELETE, DROP etc.) to the database.
        
        Use only the tools below for interacting with the database. Only use the information returned by the below tools to construct your final answer. You MUST double check your query before executing it. If you get an error while executing a query, rewrite the query and try again.
        """
        self.sql_agent_prompt_suffix = completion_request.agent.prompt_suffix or """Begin!

        Question: {input}
        Thought: I should evaluate the question to see if it is related to the database. If so, I should look at the tables in the database to see what I can query. Then I should query the schema of the most relevant tables. If not, I should provide my name and details about the types of questions I can answer. Finally, create a nice sentence that answers the question.
        {agent_scratchpad}"""

        #If the question does not seem related to the database, politely answer with your name and details about the types of questions you can answer.

        self.sql_agent = create_sql_agent(
            llm = self.llm,
            toolkit = SecureSQLDatabaseToolkit(
                db = SQLDatabaseFactory(sql_db_config = self.sql_db_config,
                                        config = config).get_sql_database(),
                llm=self.llm,
                reduce_k_below_max_tokens=True,
                username = self.sql_db_config.username, # TODO: This should be the logged in user.
                use_row_level_security = self.sql_db_config.row_level_security_enabled
            ),
            agent_type = AgentType.ZERO_SHOT_REACT_DESCRIPTION,
            verbose = True,
            prefix = PromptTemplate.from_template(self.sql_agent_prompt),
            suffix = self.sql_agent_prompt_suffix,
            agent_executor_kwargs={
                'handle_parsing_errors': True
            }
        )

        self.df = pd.read_sql(
            'SELECT * FROM RumInventory',
            create_engine(MicrosoftSQLServer(sql_db_config = self.sql_db_config,
                                             config = config).get_connection_string()),
            index_col='Id'
        )

        self.statistics_agent = create_pandas_dataframe_agent(
            llm = self.llm,
            df = self.df,
            verbose = True,
            prefix = 'You are a helpful agent designed to retrieve statistics about data in a Pandas DataFrame named "df".',
            handle_parsing_errors='Get the final answer from the output without quotes, verify it is correct, and return that as the response!'
        )

        self.python_agent = create_python_agent(
            llm = self.llm,
            tool = PythonREPLTool(),
            agent_type = AgentType.ZERO_SHOT_REACT_DESCRIPTION,
            verbose = True,
            handle_parsing_errors='Get the final answer from the output without quotes, verify it is correct, and return that as the response!'
        )

        self.tools = [
            Tool(
                name = 'product_database',
                func = self.sql_agent.run,
                description = 'Useful for answering questions about anomalies in rum bottle volumes and prices.',
                handle_tool_error = True
            )
        ]

        self.toolkit = AnomalyDetectionToolkit(df_agent=self.statistics_agent, py_agent=self.python_agent)

        self.agent_prompt_suffix = """Begin!

        Question: {input}
        Thought: I should use the tools available to determine if they input record is anomalous. Validate the output is properly formatted or try again. Finally, generate a nice sentence that answers the question.
        {agent_scratchpad}"""

        self.agent = initialize_agent(
            tools = self.tools + self.toolkit.get_tools(),
            llm = self.llm,
            agent = AgentType.ZERO_SHOT_REACT_DESCRIPTION,
            verbose = True,
            max_iterations = 10,
            early_stopping_method = 'generate',
            agent_kwargs={
                'prefix': self.agent_prompt_prefix,
                'suffix': self.agent_prompt_suffix
            },
            handle_parsing_errors='Get the final answer from the output without quotes, verify it is correct, and return that as the response!'
        )

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
        Executes an anomaly detection request.

        Parameters
        ----------
        prompt : str
            The prompt for which a completion is begin generated.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the anomaly detection completion response, 
            the user_prompt, and token utilization and execution cost details.
        """
        with get_openai_callback() as cb:
            try:
                completion = self.agent.run(prompt)
            except OutputParserException as e:
                completion = str(e)

            return CompletionResponse(
                    completion = completion.replace('Could not parse LLM output: ', '').replace('`', ''),
                    user_prompt= prompt,
                    completion_tokens = cb.completion_tokens,
                    prompt_tokens = cb.prompt_tokens,
                    total_tokens = cb.total_tokens,
                    total_cost = cb.total_cost
            )
