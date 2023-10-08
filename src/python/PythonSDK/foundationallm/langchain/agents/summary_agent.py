from foundationallm.langchain.openai_models import AzureChatLLM
from foundationallm.models.orchestration import SummaryRequest, SummaryResponse
import tiktoken
from langchain.chains.summarize import load_summarize_chain
from langchain.docstore.document import Document
from langchain.text_splitter import CharacterTextSplitter
from langchain.prompts import PromptTemplate

class SummaryAgent():
    """
    Agent for summarizing input text.
    """
        
    def __init__(self, content: SummaryRequest, llm: AzureChatLLM):
        self.prompt = PromptTemplate.from_template(content.prompt_template)
        self.text_to_summarize = content.prompt
        self.llm = llm.get_chat_model()

    def run(self) -> SummaryResponse:
        model_name = 'gpt-35-turbo'
        max_tokens = 4097
        
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
                prompt=self.prompt,
                verbose=True
            )
        else:
            summarizer_chain = load_summarize_chain(
                llm=self.llm,
                chain_type='map_reduce',
                map_prompt=self.prompt,
                combine_prompt=self.prompt,
                verbose=True
            )

        # Summarize text
        return SummaryResponse(summary=summarizer_chain.run(docs))
