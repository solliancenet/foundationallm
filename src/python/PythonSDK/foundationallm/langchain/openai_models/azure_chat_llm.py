from langchain.base_language import BaseLanguageModel
from langchain.chat_models import AzureChatOpenAI
from foundationallm.config import Configuration

class AzureChatLLM():
    def __init__(self, base_url: str, deployment_name: str, config: Configuration):
         self.openai_api_base = base_url
         self.openai_api_key = config.get_value('azure-openai-api-key')
         self.deployment_name = deployment_name
         self.model_version = '0301'
         
    def get_chat_model(self) -> BaseLanguageModel:
        return AzureChatOpenAI(
            temperature = 0,
            openai_api_base = self.openai_api_base,
            openai_api_version = '2023-05-15',
            deployment_name = self.deployment_name,
            openai_api_key = self.openai_api_key,
            openai_api_type = 'azure',
            model_version = self.model_version
        )