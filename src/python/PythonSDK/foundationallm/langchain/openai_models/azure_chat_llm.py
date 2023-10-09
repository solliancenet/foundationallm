from langchain.base_language import BaseLanguageModel
from langchain.chat_models import AzureChatOpenAI
from foundationallm.config import Configuration

class AzureChatLLM():
    config_value_base_name: str = 'foundationallm-azure-openai-api'
    
    def __init__(self, app_config: Configuration):
        self.app_config = app_config
         
    def get_chat_model(self) -> BaseLanguageModel:
        return AzureChatOpenAI(
            temperature = 0,
            openai_api_type = 'azure',
            openai_api_base = self.app_config.get_value(f'{self.config_value_base_name}-url'),
            openai_api_key =  self.app_config.get_value(f'{self.config_value_base_name}-key'),
            openai_api_version = self.app_config.get_value(f'{self.config_value_base_name}-version'),
            deployment_name = self.app_config.get_value(f'{self.config_value_base_name}-completions-deployment'),
            model_version = self.app_config.get_value(f'{self.config_value_base_name}-completions-model-version')
        )