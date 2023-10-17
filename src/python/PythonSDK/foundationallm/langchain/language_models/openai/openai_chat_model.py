from langchain.base_language import BaseLanguageModel
from langchain.chat_models import ChatOpenAI

from foundationallm.config import Configuration
import foundationallm.models.metadata.language_model as LanguageModel
from foundationallm.langchain.language_models.openai import OpenAIModelBase

class OpenAIChatModel(OpenAIModelBase):
    """OpenAI chat model."""
    config_value_base_name: str = 'FoundationaLLM:OpenAI:API'
    
    def __init__(self, language_model: LanguageModel, config: Configuration):
        """
        Initializes an Open AI chat model.

        Parameters
        ----------
        language_model: LanguageModel
            The language model metadata class.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.config = config
        self.openai_api_base = self.config.get_value[f'{self.config_value_base_name}:Endpoint']
        self.temperature = self.config.get_value[f'{self.config_value_base_name}:Temperature']
        self.openai_api_key = self.config.get_value(f'{self.config_value_base_name}:Key')
         
    def get_language_model(self) -> BaseLanguageModel:
        """
        Returns an OpenAI chat model.
        
        Returns
        -------
        BaseLanguageModel
            Returns an AzureChatOpenAI chat model.
        """
        return ChatOpenAI(
            openai_api_base = self.openai_api_base,
            openai_api_key = self.openai_api_key,
            temperature = self.temperature
        )