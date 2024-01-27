from typing import List
import tiktoken

from langchain.chains.combine_documents.base import BaseCombineDocumentsChain
from langchain.chains.summarize import load_summarize_chain
from langchain.text_splitter import CharacterTextSplitter
from langchain_community.callbacks import get_openai_callback
from langchain_core.documents import Document
from langchain_core.prompts import PromptTemplate

from foundationallm.config import Configuration
from foundationallm.langchain.agents.agent_base import AgentBase
from foundationallm.langchain.language_models import LanguageModelBase
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse

class SummaryAgent(AgentBase):
    """
    Agent for summarizing input text.
    """

    def __init__(
            self,
            completion_request: CompletionRequest,
            llm: LanguageModelBase,
            config: Configuration):
        """
        Initializes a SummaryAgent

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
        self.config = config
        self.summarizer_chain_prompt = PromptTemplate.from_template(
                                        completion_request.agent.prompt_prefix)
        self.llm = llm.get_completion_model(completion_request.language_model)
        self.model_name = self.config.get_value("FoundationaLLM:LangChain:Summary:ModelName")
        self.max_tokens = self.config.get_value("FoundationaLLM:LangChain:Summary:MaxTokens")

    def __get_text_as_documents(self, prompt: str) -> List[Document]:
        """
        Splits text into smaller parts and creates smaller documents
            to split the summarization task into smaller jobs.

        Parameters
        ----------
        prompt : str
            The prompt for which a summary completion is begin generated.

        Returns
        -------
        List[Document]
            Returns an array of documents.
        """
        text_splitter = CharacterTextSplitter()
        texts = text_splitter.split_text(prompt)

        # Create multiple documents
        return [Document(page_content=t) for t in texts]

    def __get_summarizer_chain(self, prompt: str) -> BaseCombineDocumentsChain:
        """
        Builds a LangChain summarizer chain to use for summarizing the user prompt.

        Parameters
        ----------
        prompt : str
            The prompt for which a summary completion is begin generated.
        
        Returns
        -------
        BaseCombineDocumentsChain
            LangChain chain for combining documents and summarizing text within them.
        """
        model_name = self.model_name
        max_tokens = int(self.max_tokens)

        encoding = tiktoken.encoding_for_model(model_name)
        num_tokens = len(encoding.encode(prompt))

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

    def run(self, prompt: str) -> CompletionResponse:
        """
        Executes a completion request using a summarizer agent.

        Parameters
        ----------
        prompt : str
            The prompt for which a summary completion is begin generated.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the generated summary, the user_prompt,
            and token utilization and execution cost details.
        """
        docs = self.__get_text_as_documents(prompt=prompt)
        summarizer_chain = self.__get_summarizer_chain(prompt=prompt)

        # Summarize text
        with get_openai_callback() as cb:
            return CompletionResponse(
                completion = summarizer_chain.run(docs),
                user_prompt = prompt,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )
