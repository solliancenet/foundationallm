from typing import List

from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain_community.callbacks import get_openai_callback
from langchain_community.document_loaders import AzureBlobStorageFileLoader, AzureBlobStorageContainerLoader
from langchain_community.vectorstores.chroma import Chroma
from langchain_core.documents import Document
from langchain_core.language_models import BaseLanguageModel
from langchain_core.output_parsers import StrOutputParser
from langchain_core.prompts import PromptTemplate
from langchain_core.runnables import RunnablePassthrough, RunnableLambda

from foundationallm.config import Configuration
from foundationallm.langchain.agents.agent_base import AgentBase
from foundationallm.langchain.data_sources.blob.blob_storage_configuration import BlobStorageConfiguration
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from foundationallm.langchain.message_history import build_message_history

vector_store = {}

class BlobStorageAgent(AgentBase):
    """
    Agent for reading, indexing, and querying blobs from a blob storage container.
    """

    def __init__(
            self,
            completion_request: CompletionRequest,
            llm: BaseLanguageModel,
            config: Configuration):
        """
        Initializes a blob storage in-memory query agent.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent and data source metadata.
        llm: BaseLanguageModel
            The language model class to use for embedding and completion.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        ds_config = {}
        for ds in completion_request.data_sources:
            ds_config: BlobStorageConfiguration = ds.configuration
        self.llm = llm.get_completion_model(completion_request.language_model)
        self.embedding = llm.get_embedding_model(completion_request.embedding_model)
        self.prompt_prefix = completion_request.agent.prompt_prefix
        self.connection_string = config.get_value(ds_config.connection_string_secret)
        self.container_name = ds_config.container
        self.file_names = ds_config.files
        self.message_history = completion_request.message_history
        self.full_prompt = ""

    def __get_vector_index(self) -> Chroma:
        """
        Creates a vector index from files in the indicated blob storage container and files list
        """

        if self.container_name in vector_store:
            return vector_store[self.container_name]

        docs = []
        if "*" in self.file_names:
            # Load all files in the container
            loader = AzureBlobStorageContainerLoader(conn_str=self.connection_string,
                                                     container=self.container_name)
            docs.extend(loader.load())
        else:
            # Load specific files
            for file_name in self.file_names:
                loader = AzureBlobStorageFileLoader(conn_str=self.connection_string,
                                                    container=self.container_name,
                                                    blob_name=file_name)
                docs.extend(loader.load())

        text_splitter = RecursiveCharacterTextSplitter(chunk_size=1000, chunk_overlap=200)
        splits = text_splitter.split_documents(docs)

        index = Chroma.from_documents(documents=splits, embedding=self.embedding,
                                      collection_name=self.container_name)
        vector_store[self.container_name] = index
        return index

    def __format_docs(self, docs:List[Document]) -> str:
        """
        Generates a formatted string from a list of documents for use
        as the context for the completion request.
        """
        return "\n\n".join(doc.page_content for doc in docs)

    def __record_full_prompt(self, prompt: str) -> str:
        """
        Records the full prompt for the completion request.

        Parameters
        ----------
        prompt : str
            The prompt that is populated with context.
        
        Returns
        -------
        str
            Returns the full prompt.
        """
        self.full_prompt = prompt
        return prompt

    def run(self, prompt: str) -> CompletionResponse:
        """
        Executes a completion request by querying the vector index with the user prompt.

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

        with get_openai_callback() as cb:
            index = self.__get_vector_index()
            retriever = index.as_retriever()
            prompt_builder = self.prompt_prefix + build_message_history(self.message_history) + \
                        "\n\nQuestion: {question}\n\nContext: {context}\n\nAnswer:"
            custom_prompt = PromptTemplate.from_template(prompt_builder)

            rag_chain = (
                { "context": retriever | self.__format_docs, "question": RunnablePassthrough()}
                | custom_prompt
                | RunnableLambda(self.__record_full_prompt)
                | self.llm
                | StrOutputParser()
            )

            return CompletionResponse(
                completion = rag_chain.invoke(prompt),
                user_prompt = prompt,
                full_prompt = self.full_prompt.text,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )
