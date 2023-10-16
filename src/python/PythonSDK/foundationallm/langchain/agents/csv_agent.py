from langchain.agents import create_csv_agent
from langchain.agents.agent_types import AgentType
from langchain.callbacks import get_openai_callback

from foundationallm.config import Configuration
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.data_sources.csv import CSVConfiguration
from foundationallm.langchain.language_models import LanguageModelBase
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse

class CSVAgent(AgentBase):
    """
    Agent for analyzing the contents of delimited files (e.g., CSV).
    """
    
    def __init__(self, completion_request: CompletionRequest, llm: LanguageModelBase, config: Configuration):
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
        self.agent_prompt_prefix = completion_request.agent.prompt_template
        self.llm = llm.get_language_model()
        self.data_source_config: CSVConfiguration = completion_request.data_source.configuration
        if self.data_source_config.path_value_is_secret:
            self.source_file_path = config.get_value(self.data_source_config.source_file_path)
        else:
            self.source_file_path = self.data_source_config.source_file_path
        
        self.agent = create_csv_agent(
            llm = self.llm,
            path = self.source_file_path,
            verbose = True,
            agent_type = AgentType.ZERO_SHOT_REACT_DESCRIPTION,
            prefix = self.agent_prompt_prefix
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
                user_prompt= prompt,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )