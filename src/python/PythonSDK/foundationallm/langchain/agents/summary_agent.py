import tiktoken
from langchain.callbacks import get_openai_callback
from langchain.chains.summarize import load_summarize_chain
from langchain.docstore.document import Document
from langchain.prompts import PromptTemplate
from langchain.text_splitter import CharacterTextSplitter

from foundationallm.config import Configuration
from foundationallm.langchain.agents.agent_base import AgentBase
from foundationallm.langchain.language_models.chat_models import AzureChatOpenAILanguageModel
from foundationallm.models.orchestration import SummaryRequest, SummaryResponse

class SummaryAgent(AgentBase):
    """
    Agent for summarizing input text.
    """
        
    def __init__(self, summary_request: SummaryRequest, llm: AzureChatOpenAILanguageModel, app_config: Configuration):
        self.summarizer_chain_prompt = PromptTemplate.from_template(summary_request.agent.prompt_template)
        self.user_prompt = summary_request.user_prompt
        self.llm = llm.get_language_model()
        self.model_name = app_config.get_value('foundationallm-langchain-summary-model-name')
        self.max_tokens = app_config.get_value('foundationallm-langchain-summary-model-max-tokens')
        
    def __get_text_as_documents(self):
        text_splitter = CharacterTextSplitter()
        texts = text_splitter.split_text(self.user_prompt)

        # Create multiple documents
        return [Document(page_content=t) for t in texts]
    
    def __get_summarizer_chain(self):
        model_name = self.model_name
        max_tokens = int(self.max_tokens)
        
        encoding = tiktoken.encoding_for_model(model_name)
        num_tokens = len(encoding.encode(self.user_prompt))
    
        # Summarize output filter
        if num_tokens < max_tokens:
            return load_summarize_chain(
                llm=self.llm,
                chain_type='stuff',
                prompt=self.summarizer_chain_prompt,
                verbose=True
            )
        else:
            return load_summarize_chain(
                llm=self.llm,
                chain_type='map_reduce',
                map_prompt=self.summarizer_chain_prompt,
                combine_prompt=self.summarizer_chain_prompt,
                verbose=True
            )

    def run(self) -> SummaryResponse:
        docs = self.__get_text_as_documents()
        summarizer_chain = self.__get_summarizer_chain()

        # Summarize text
        with get_openai_callback() as cb:
            return SummaryResponse(
                summary=summarizer_chain.run(docs),
                user_prompt= self.user_prompt,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )
