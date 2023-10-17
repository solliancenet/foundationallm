from langchain.base_language import BaseLanguageModel
from langchain.llms import AzureOpenAI

from foundationallm.config import Configuration
import foundationallm.models.metadata.language_model as LanguageModel
from foundationallm.langchain.language_models.openai import OpenAIModelBase, AzureOpenAIAPIType

class AzureTextCompletionModel(OpenAIModelBase):
    """Azure OpenAI text completion model."""
    config_value_base_name: str = 'FoundationaLLM:AzureOpenAI:API'
    deployment_name: str
    
    def __init__(self, language_model: LanguageModel, config: Configuration):
        """
        Initializes the Azure Open AI text completion model.

        Parameters
        ----------
        language_model: LanguageModel
            The language model metadata class.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.config = config        
        self.openai_api_base = self.config.get_value[f'{self.config_value_base_name}:Endpoint']
        self.openai_api_version = self.config.get_value[f'{self.config_value_base_name}:Version']
        self.deployment_name = self.config.get_value[f'{self.config_value_base_name}:Completions:DeploymentName']
        self.max_tokens = self.config.get_value[f'{self.config_value_base_name}:Completions:MaxTokens']
        self.temperature = self.config.get_value[f'{self.config_value_base_name}:Completions:Temperature']
        self.openai_api_type = AzureOpenAIAPIType.AZURE
        self.openai_api_key = self.config.get_value(f'{self.config_value_base_name}:Key')
         
    def get_language_model(self) -> BaseLanguageModel:
        """
        Returns the Azure OpenAI text completion model.
        
        Returns
        -------
        BaseLanguageModel
            Returns an AzureChatOpenAI chat model.
        """
        return AzureOpenAI(
            temperature = self.temperature,
            openai_api_base = self.openai_api_base,
            openai_api_key = self.openai_api_key,
            openai_api_type = self.openai_api_type,
            openai_api_version = self.openai_api_version,
            deployment_name = self.deployment_name,
            #max_tokens = self.max_tokens
        )