from azure.core.credentials import AzureKeyCredential
from langchain_core.retrievers import BaseRetriever
from foundationallm.config import Configuration
from foundationallm.langchain.language_models.openai import OpenAIModel
from foundationallm.models.language_models import EmbeddingModel, LanguageModelType, LanguageModelProvider
from foundationallm.resources import ResourceProvider
from .azure_ai_search_service_retriever import AzureAISearchServiceRetriever

class RetrieverFactory:
    """
    Factory class for determine which retriever to use.
    """
    def __init__(
                self,
                indexing_profile_resource_id: str,
                embedding_profile_resource_id:str,
                config: Configuration,
                resource_provider: ResourceProvider
                ):
        self.config = config
        self.resource_provider = resource_provider
        self.indexing_profile = resource_provider.get_resource(indexing_profile_resource_id)
        self.embedding_profile = resource_provider.get_resource(embedding_profile_resource_id)        

    def get_retriever(self) -> BaseRetriever:
        """
        Retrieves the retriever to use for completion requests.
        
        Returns
        -------
        BaseRetriever
            Returns the concrete initialization of a vectorstore retriever.
        """

        # use embedding profile to build the embedding model (currently only supporting Azure OpenAI)         
        #embedding_model_type = self.embedding_profile["text_embedding"]
        #embedding_model = None
        #match embedding_model_type:            
        #    case "SemanticKernelTextEmbedding": # same as Azure Open AI Embedding
        e_model = EmbeddingModel(
            type = LanguageModelType.OPENAI,
            provider = LanguageModelProvider.MICROSOFT,
            # the OpenAI model uses config to retrieve the app config values - pass in the keys
            deployment = self.embedding_profile["configuration_references"]["deployment_name"],
            api_endpoint = self.embedding_profile["configuration_references"]["endpoint"],
            api_key = self.embedding_profile["configuration_references"]["api_key"],
            api_version = self.embedding_profile["configuration_references"]["api_version"]
        )
        oai_model = OpenAIModel(config = self.config)
        embedding_model = oai_model.get_embedding_model(e_model)
                  
        # use indexing profile to build the retriever (current only supporting Azure AI Search)
        #vector_store_type = self.indexing_profile["indexer"]        
        #match vector_store_type:
        #    case "AzureAISearchIndexer":

        retriever = AzureAISearchServiceRetriever( 
            endpoint = self.config.get_value(self.indexing_profile["configuration_references"]["endpoint"]),
            index_name = self.indexing_profile["settings"]["index_name"],
            top_n = self.indexing_profile["settings"]["top_n"],
            embedding_field_name = self.indexing_profile["settings"]["embedding_field_name"],
            text_field_name = self.indexing_profile["settings"]["text_field_name"],
            filters = self.indexing_profile["settings"]["filters"],
            credential = AzureKeyCredential(
                self.config.get_value(
                    self.indexing_profile["configuration_references"]["query_api_key"]
                )
            ),
            embedding_model = embedding_model
        )
        return retriever
