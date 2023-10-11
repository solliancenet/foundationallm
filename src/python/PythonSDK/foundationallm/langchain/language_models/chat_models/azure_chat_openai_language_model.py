from langchain.base_language import BaseLanguageModel
from langchain.chat_models import AzureChatOpenAI
from foundationallm.config import Configuration
from foundationallm.langchain.language_models.chat_models import ChatModelBase

class AzureChatOpenAILanguageModel(ChatModelBase):
    config_value_base_name: str = 'foundationallm-azure-openai-api'
    deployment_name: str
    model_version: str
    
    def __init__(self, config: Configuration):
        self.config = config
        self.openai_api_base = self.config.get_value(f'{self.config_value_base_name}-url')
        self.openai_api_key = self.config.get_value(f'{self.config_value_base_name}-key')
        self.openai_api_type = 'azure'
        self.openai_api_version = self.config.get_value(f'{self.config_value_base_name}-version')
        self.deployment_name = self.config.get_value(f'{self.config_value_base_name}-completions-deployment')
        self.model_version = self.config.get_value(f'{self.config_value_base_name}-completions-model-version')
        #self.temperature ???
         
    def get_language_model(self) -> BaseLanguageModel:
        return AzureChatOpenAI(
            temperature = self.temperature,
            openai_api_base = self.openai_api_base,
            openai_api_key = self.openai_api_key,
            openai_api_type = self.openai_api_type,
            openai_api_version = self.openai_api_version,
            deployment_name = self.deployment_name,
            model_version = self.model_version
        )