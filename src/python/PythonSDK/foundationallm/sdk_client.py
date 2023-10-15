from langchain.base_language import BaseLanguageModel
from foundationallm.config import Configuration
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.credentials import Credential
import foundationallm.models.metadata.language_model as LanguageModel
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import AgentFactory


class SDKClient:
    """Client that acts as the entry point for interacting with the FoundationaLLM Python SDK."""
    
    def __init__(self, credential: Credential):
        """
        Initializes the SDK client
        
        Parameters
        ----------
        credential : Credential
            The type of credential to use for authenticating against the SDK.
        """
        self.credential = credential
        self.config = self.get_configuration()
    
    def create_agent(self, completion_request: CompletionRequest):
        """Creates an agent for executing completion requests."""
        llm = self.get_llm(language_model=completion_request.language_model)
        agent = AgentFactory(completion_request=completion_request, llm=llm, config=self.config)
        return agent.get_agent()
    
    def get_configuration(self):
        """Retrieves an application configuration object for getting app settings."""
        keyvault_name = Configuration().get_value(key='foundationallm-keyvault-name')
        return Configuration(keyvault_name=keyvault_name, credential=self.credential)
    
    def get_llm(self, language_model: LanguageModel) -> BaseLanguageModel:
        """
        Retrieves the language model to use for the completion.
        
        Parameters
        ----------
        language_model : LanguageModel
            Language model metadata object used to create the BaseLanguageModel LLM object.
        
        Returns
        -------
        BaseLanguageModel
            Returns the language model to use for the completion.
        """
        return LanguageModelFactory(language_model=language_model, config=self.config).get_llm()