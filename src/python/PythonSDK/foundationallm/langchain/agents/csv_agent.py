from langchain.agents import create_csv_agent
from langchain.agents.agent_types import AgentType
from langchain.callbacks import get_openai_callback
from langchain.prompts import PromptTemplate

from foundationallm.config import Configuration
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.data_sources.csv import CSVConfig
from foundationallm.langchain.language_models.chat_models import AzureChatOpenAILanguageModel
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse

class CSVAgent(AgentBase):
    """
    Agent for analyzing the contents of delimited files (e.g., CSV).
    """
    
    def __init__(self, completion_request: CompletionRequest, llm: AzureChatOpenAILanguageModel, app_config: Configuration):
        self.agent_prompt_prefix = completion_request.agent.prompt_template #PromptTemplate.from_template(completion_request.agent.prompt_template)
        self.user_prompt = completion_request.user_prompt
        self.llm = llm.get_language_model()
        self.data_source_config: CSVConfig = completion_request.data_source.configuration
        if self.data_source_config.path_value_is_secret:
            self.source_file_path = app_config.get_value(self.data_source_config.source_file_path)
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