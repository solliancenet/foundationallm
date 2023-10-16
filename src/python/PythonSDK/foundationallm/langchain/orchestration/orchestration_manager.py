from langchain.base_language import BaseLanguageModel
from foundationallm.credentials import Credential
from foundationallm.config import Configuration
from foundationallm.models.metadata.language_model import LanguageModel
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import AgentFactory, AgentBase
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse

class OrchestrationManager:
    """Client that acts as the entry point for interacting with the FoundationaLLM Python SDK."""
    
    def __init__(self, completion_request: CompletionRequest, credential: Credential):
        """
        Initializes an instance of the OrchestrationManager.
        
        Parameters
        ----------
        completion_request : CompletionRequest
            The CompletionRequest is the metadata object containing the details needed for
            the OrchestrationManager to assemble an agent with a language model and data source.
        credential : Credential
            The type of credential to use for authenticating against the SDK.
        """
        self.completion_request = completion_request
        self.credential = credential
        self.config = self.__get_configuration()
        self.llm = self.__get_llm(language_model=completion_request.language_model)
        self.agent = self.__create_agent(completion_request=completion_request)
    
    def __create_agent(self, completion_request: CompletionRequest) -> AgentBase:
        """Creates an agent for executing completion requests."""
        agent = AgentFactory(completion_request=completion_request, llm=self.llm, config=self.config)
        return agent.get_agent()
    
    def __get_configuration(self):
        """Retrieves an application configuration object for getting app settings."""
        keyvault_name = Configuration().get_value(key='foundationallm-keyvault-name')
        return Configuration(keyvault_name=keyvault_name, credential=self.credential)
    
    def __get_llm(self, language_model: LanguageModel) -> BaseLanguageModel:
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
    
    def run(self, prompt: str) -> CompletionResponse:
        """
        Executes a completion request against the LanguageModel using 
        the LangChain agent assembled by the OrchestrationManager.
        
        Parameters
        ----------
        prompt : str
            The prompt for which a completion is being generated.
            
        Returns
        -------
        CompletionResponse
            Object containing the completion response and token usage details.
        """
        return self.agent.run(prompt=prompt)