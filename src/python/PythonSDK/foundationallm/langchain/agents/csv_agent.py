from tabnanny import verbose
from langchain.agents import create_csv_agent
from langchain.agents.agent_types import AgentType
from langchain.callbacks import get_openai_callback
from langchain.prompts import PromptTemplate

from foundationallm.langchain.openai_models.azure_chat_llm import AzureChatLLM
from foundationallm.config import Configuration
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse

class CSVAgent():
    """
    Agent for querying the contents of delimited files (e.g., CSV).
    """
    
    def __init__(self, completion_request: CompletionRequest, llm: AzureChatLLM, app_config: Configuration):
        self.user_prompt = completion_request.user_prompt
        self.llm = llm.get_chat_model()
        self.source_csv_file = app_config.get_value('foundationallm-langchain-csv-file-url')
        
        self.prompt_prefix = """
            You are an analytics agent named Khalil.
            You help users answer their questions about survey data. If the user asks you to answer any other question besides questions about the data, politely suggest that go ask a human as you are a very focused agent.
            You are working with a pandas dataframe in Python that contains the survey data. The name of the dataframe is `df`.    
            You should use the tools below to answer the question posed of you:"""

        self.agent = create_csv_agent(
            llm = self.llm,
            path = self.source_csv_file, # TODO: Get from CompletionRequest? The class should have the metadata object on it with the object config.
            verbose = True,
            agent_type = AgentType.ZERO_SHOT_REACT_DESCRIPTION,
            prefix = PromptTemplate.from_template(self.prompt_prefix) # TODO: This should also come from CompletionRequest.
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