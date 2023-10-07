from langchain.chat_models import AzureChatOpenAI
from pydantic import confloat
from foundationallm.config import Configuration

class AzureChat():
    def __init__(self, openai_api_type: str, base_url: str, deployment_name: str, config: Configuration):
        self.openai_api_type = openai_api_type
        self.base_url = base_url
        self.deployment_name = deployment_name
        self.openai_api_key = config.get_value('azure-openai-api-key')
        
    def get_llm(self, temperature: confloat(ge=0, le=1) = 0): # TODO: confloat isn't working properly here
        return AzureChatOpenAI(
            temperature = temperature,
            openai_api_base = self.base_url,
            openai_api_version = '2023-05-15',
            deployment_name = self.deployment_name,
            openai_api_key = self.openai_api_key,
            openai_api_type = self.openai_api_type,
            model_version = '0301'
        )