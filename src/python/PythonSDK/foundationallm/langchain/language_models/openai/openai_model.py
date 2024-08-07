from langchain_core.embeddings import Embeddings
from langchain_openai import AzureOpenAIEmbeddings, OpenAIEmbeddings

from foundationallm.config import Configuration
from foundationallm.langchain.language_models import LanguageModelBase
from foundationallm.models.language_models import (
    EmbeddingModel,
    LanguageModelProvider
)

class OpenAIModel(LanguageModelBase):
    """OpenAI Completion model."""
    config_value_base_name: str

    def __init__(self, config: Configuration):
        """
        Initializes an OpenAI completion language model.
        
        Parameters
        ----------
        language_model: LanguageModel
            The language model metadata class.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.config = config

    # def get_completion_model(self, language_model: LanguageModel) -> BaseLanguageModel:
    #     """
    #     Returns an OpenAI completion model.
        
    #     Returns
    #     -------
    #     BaseLanguageModel
    #         Returns an OpenAI completion model.
    #     """
    #     use_chat = language_model.use_chat

    #     openai_base_url = self.config.get_value(language_model.api_endpoint)
    #     openai_api_key = self.config.get_value(language_model.api_key)
    #     openai_api_version = self.config.get_value(language_model.api_version)

    #     # Overridable values
    #     azure_deployment_name = language_model.deployment
    #     temperature = language_model.temperature or 0.0

    #     if language_model.provider == LanguageModelProvider.MICROSOFT:        
    #         if use_chat:
    #             return AzureChatOpenAI(
    #                 api_key = openai_api_key,
    #                 api_version = openai_api_version,
    #                 azure_deployment = azure_deployment_name,
    #                 azure_endpoint = openai_base_url,
    #                 #max_tokens = self.config.get_value(f'{config_value_base_name}:Completions:MaxTokens'),
    #                 model_version = self.config.get_value(language_model.version),
    #                 temperature = temperature
    #             )
    #         else:
    #             return AzureOpenAI(
    #                 api_key = openai_api_key,
    #                 api_version = openai_api_version,
    #                 azure_deployment = azure_deployment_name,
    #                 azure_endpoint = openai_base_url,
    #                 #max_tokens = self.config.get_value(f'{config_value_base_name}:Completions:MaxTokens'),
    #                 temperature = temperature
    #             )
    #     else:
    #         if use_chat:
    #             return ChatOpenAI(
    #                 base_url = openai_base_url,
    #                 api_key = openai_api_key,
    #                 temperature = temperature
    #             )
    #         else:
    #             return OpenAI(
    #                 base_url = openai_base_url,
    #                 api_key = openai_api_key,
    #                 temperature = temperature
    #             )

    def get_embedding_model(self, embedding_model: EmbeddingModel) -> Embeddings:
        """
        Retrieves the OpenAI embedding model.
        
        Returns
        -------
        Embeddings
            Returns an OpenAI embeddings model.
        """
        if embedding_model is None:
            raise ValueError('Expected populated embedding_model, got None.')

        if embedding_model.provider == LanguageModelProvider.MICROSOFT:
            return AzureOpenAIEmbeddings(
                azure_ad_token = embedding_model.api_key,
                openai_api_version = embedding_model.api_version,
                deployment = embedding_model.deployment,
                azure_endpoint = embedding_model.api_endpoint,
                chunk_size = embedding_model.chunk_size
            )
        else:
            return OpenAIEmbeddings(
                api_key = embedding_model.api_key,
                api_version = embedding_model.api_version,
                base_url = embedding_model.api_endpoint,
                chunk_size = embedding_model.chunk_size or 500,
                deployment = embedding_model.deployment,
                model = embedding_model.model
            )
