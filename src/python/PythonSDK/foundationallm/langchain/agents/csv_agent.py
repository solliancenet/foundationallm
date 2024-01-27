from io import StringIO
from operator import itemgetter
import pandas as pd

from langchain.agents import AgentExecutor, ZeroShotAgent
from langchain.chains import LLMChain
from langchain.memory import ConversationBufferMemory
from langchain_community.callbacks import get_openai_callback
from langchain_experimental.tools.python.tool import PythonAstREPLTool

from foundationallm.config import Configuration
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.language_models import LanguageModelBase
from foundationallm.langchain.data_sources.blob import BlobStorageConfiguration
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from foundationallm.storage import BlobStorageManager

class CSVAgent(AgentBase):
    """
    Agent for analyzing the contents of delimited files (e.g., CSV).
    """

    def __init__(
        self,
        completion_request: CompletionRequest,
        llm: LanguageModelBase,
        config: Configuration
    ):
        """
        Initializes a CSV agent.

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
        ds = {}
        for data_source in completion_request.data_sources:
            ds = data_source

        ds_config: BlobStorageConfiguration = ds.configuration
        self.prompt_prefix = completion_request.agent.prompt_prefix
        self.prompt_suffix = completion_request.agent.prompt_suffix
        self.llm = llm.get_completion_model(completion_request.language_model)
        self.message_history = completion_request.message_history
        
        df_locals, df_names, prompt_suffix_parts = self.__build_python_repl_tool(config, ds_config)

        tools = [
            PythonAstREPLTool(locals = df_locals)
        ]

        memory = ConversationBufferMemory(memory_key="chat_history") #, return_messages=True)
        # #Add previous messages to memory
        # for i in range(0, len(self.message_history), 2):
        #     history_pair = itemgetter(i,i+1)(self.message_history)
        #     for message in history_pair:
        #         if message.sender.lower() == 'user':
        #             user_input = message.text
        #         else:
        #             ai_output = message.text
        #     memory.save_context({"input": user_input}, {"output": ai_output})

        self.prompt_prefix = completion_request.agent.prompt_prefix
        self.prompt_prefix += f'\nYou are working with {len(df_names)} pandas dataframe{"s"[:len(df_names)^1]} in Python named {", ".join(df_names)}.'
        self.prompt_prefix += '\nDo not include the names of pandas dataframes in your responses!'
        self.prompt_prefix += '\nYou should use the tool below to answer the question posed of you:'

        self.prompt_suffix = completion_request.agent.prompt_suffix + '\n\n' if completion_request.agent.prompt_suffix is not None and len(completion_request.agent.prompt_suffix) > 0 else ''
        self.prompt_suffix += '\n\n'.join(prompt_suffix_parts)
        self.prompt_suffix += '\n\nBegin!\n\n{chat_history}\n\nQuestion: {input}\n{agent_scratchpad}'

        FORMAT_INSTRUCTIONS = """Use the following format:
Question: the input question you must answer
Thought: you should always think about what to do
Action: the action to take, can only be one of: {tool_names}
Action Input: the input to the action, never add backticks "`" around the action input
Observation: the result of the action
... (this Thought/Action/Action Input/Observation can repeat N times)
Thought: I now know the final answer
Final Answer: the final answer to the original input question
Please respect the order of the steps Thought/Action/Action Input/Observation
"""

        input_variables = ['input', 'chat_history', 'agent_scratchpad']

        prompt = ZeroShotAgent.create_prompt(
            tools,
            prefix = self.prompt_prefix,
            suffix = self.prompt_suffix,
            format_instructions = FORMAT_INSTRUCTIONS,
            input_variables = input_variables
        )

        zsa = ZeroShotAgent(
            llm_chain=LLMChain(llm=self.llm, prompt=prompt),
            allowed_tools=[tool.name for tool in tools]
        )
        
        self.agent = AgentExecutor.from_agent_and_tools(
            agent=zsa,
            tools=tools,
            verbose=True,
            memory=memory,
            handle_parsing_errors='Check your output and make sure it conforms!'
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

    def __build_python_repl_tool(self, config, ds_config):
        df_names = []
        df_locals = {}
        prompt_suffix_parts = []
        all_files = ds_config.files
        # Reduce file list to only .csv files to prevent errors.
        csv_files = [file for file in all_files if file.lower().endswith('.csv')]
        storage_manager = BlobStorageManager(
            blob_connection_string = config.get_value(
                ds_config.connection_string_secret
            ),
            container_name = ds_config.container
        )
        
        for idx, file in enumerate(csv_files, start=1):
            file_content = storage_manager.read_file_content(file).decode('utf-8')
            buffer = StringIO(file_content)

            df = pd.read_csv(buffer)
            df_name = f'df{idx}'
            df_names.append(df_name)
            df_locals[df_name] = df
            
            prompt_suffix_parts.append(f'Result of `print({df_name}.head())` for {df_name}:\n{df.head(1).to_markdown(tablefmt="plain")}')

        return df_locals, df_names, prompt_suffix_parts
     
    def run(self, prompt: str) -> CompletionResponse:
        """
        Executes a query against the contents of a CSV file.
        
        Parameters
        ----------
        prompt : str
            The prompt for which a completion is begin generated.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the CSV file query completion response, 
            the user_prompt, and token utilization and execution cost details.
        """
        with get_openai_callback() as cb:
            return CompletionResponse(
                completion = self.agent.run(prompt),
                user_prompt = prompt,
                full_prompt = self.prompt_template,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )
