from pydantic import BaseModel
from foundationallm.langchain.openai_models import AzureChat
import tiktoken
from langchain.chains.summarize import load_summarize_chain
from langchain.docstore.document import Document
from langchain.text_splitter import CharacterTextSplitter
from langchain.prompts import PromptTemplate

class SummaryAgent():
    """
    """
        
    def __init__(self, prompt_template: str, llm: AzureChat):
        self.prompt = PromptTemplate.from_template(prompt_template)
        self.llm = llm.get_llm(temperature=0)

    def run(self, text: str) -> str:
        model_name = 'gpt-35-turbo'
        max_tokens = 4097
        
        text_splitter = CharacterTextSplitter()
        texts = text_splitter.split_text(text)

        # Create multiple documents
        docs = [Document(page_content=t) for t in texts]

        encoding = tiktoken.encoding_for_model(model_name)
        num_tokens = len(encoding.encode(text))
    
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
        return summarizer_chain.run(docs)
