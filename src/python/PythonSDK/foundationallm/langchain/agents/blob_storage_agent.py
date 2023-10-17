from typing import List
from foundationallm.config import Configuration
from langchain.base_language import BaseLanguageModel
from langchain.callbacks import get_openai_callback
from foundationallm.langchain.agents.agent_base import AgentBase
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from langchain.document_loaders import AzureBlobStorageFileLoader, AzureBlobStorageContainerLoader
from langchain.indexes import VectorstoreIndexCreator
from langchain.indexes.vectorstore import VectorStoreIndexWrapper

class BlobStorageAgent(AgentBase):
    """
    Agent for reading, indexing, and querying blobs from a blob storage container.
    """
        
    def __init__(self, completion_request: CompletionRequest, llm: BaseLanguageModel, config: Configuration):
        """
        Initializes a blob storage in-memory query agent.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent and data source metadata.       
        """
        self.llm = llm.get_language_model()
        self.prompt_prefix = completion_request.agent.prompt_template
        self.connection_string = config.get_value(completion_request.data_source.configuration.connection_string_secret)        
        self.container_name = completion_request.data_source.configuration.container        
        self.file_names = completion_request.data_source.configuration.files        
        
    def __get_vector_index(self) -> VectorStoreIndexWrapper:
        """
        Creates a vector index from files in the indicated blob storage container and files list
        """
        loaders = []    
        if "*" in self.file_names:
            # Load all files in the container
            loaders.append(AzureBlobStorageContainerLoader(conn_str=self.connection_string, container=self.container_name))
        else:
            # Load specific files
            for file_name in self.file_names:
                loaders.append(AzureBlobStorageFileLoader(conn_str=self.connection_string, container=self.container_name, blob_name=file_name))
        
        # Optional parameters for VectorStoreIndexCreator: embeddings (defaults to langchain's own embeddings), 
        #                               text_splitter(defaults to TextSplitter), vectorstore_cls (defaults to Chroma)
        index = VectorstoreIndexCreator().from_loaders(loaders)
        return index
             
        # alternative if you want to deal with documents
        #documents = []    
        #if "*" in self.file_names:
        #    # Load all files in the container
        #    documents.extend(AzureBlobStorageContainerLoader(conn_str=self.connection_string, container=self.container_name).load_and_split())
        #else:
        #    # Load specific files
        #    for file_name in self. file_names:
        #        documents.extend(AzureBlobStorageFileLoader(conn_str=self.connection_string, container=self.container_name, blob_name=file_name).load_and_split())
        #return documents
 
       
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
        index = self.__get_vector_index()
        completion = index.query(self.prompt_prefix +"\n"+ prompt + "\n", self.llm)
       
        with get_openai_callback() as cb:
            return CompletionResponse(
                completion = completion,
                user_prompt = prompt,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )
