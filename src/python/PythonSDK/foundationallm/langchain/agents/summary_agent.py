import tiktoken
from langchain.chains.summarize import load_summarize_chain
from langchain.docstore.document import Document
from langchain.text_splitter import CharacterTextSplitter
from langchain.prompts import PromptTemplate

from foundationallm.config import Configuration
from foundationallm.langchain.openai_models import AzureChatLLM
from foundationallm.models.orchestration import SummaryRequest, SummaryResponse

class SummaryAgent():
    """
    Agent for summarizing input text.
    """
        
    def __init__(self, summary_request: SummaryRequest, llm: AzureChatLLM, app_config: Configuration):
        self.summarizer_chain_prompt = PromptTemplate.from_template(summary_request.prompt_template)
        self.text_to_summarize = summary_request.user_prompt
        self.llm = llm.get_chat_model()
        self.model_name = app_config.get_value('foundationallm-langchain-summary-model-name')
        self.max_tokens = app_config.get_value('foundationallm-langchain-summary-model-max-tokens')

    def run(self) -> SummaryResponse:
        model_name = self.model_name
        max_tokens = int(self.max_tokens)
        
        text_splitter = CharacterTextSplitter()
        texts = text_splitter.split_text(self.text_to_summarize)

        # Create multiple documents
        docs = [Document(page_content=t) for t in texts]

        encoding = tiktoken.encoding_for_model(model_name)
        num_tokens = len(encoding.encode(self.text_to_summarize))
    
        # Summarize output filter
        if num_tokens < max_tokens:
            summarizer_chain = load_summarize_chain(
                llm=self.llm,
                chain_type='stuff',
                prompt=self.summarizer_chain_prompt,
                verbose=True
            )
        else:
            summarizer_chain = load_summarize_chain(
                llm=self.llm,
                chain_type='map_reduce',
                map_prompt=self.summarizer_chain_prompt,
                combine_prompt=self.summarizer_chain_prompt,
                verbose=True
            )

        # Summarize text
        return SummaryResponse(summary=summarizer_chain.run(docs))
