from langchain_core.language_models import BaseLanguageModel

from foundationallm.config import Configuration, Context
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import AgentFactory, AgentBase
from foundationallm.models.language_models import LanguageModel
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse

class OrchestrationManager:
    """Client that acts as the entry point for interacting with the FoundationaLLM Python SDK."""

    def __init__(self, completion_request: CompletionRequest,
                 configuration: Configuration, context: Context):
        """
        Initializes an instance of the OrchestrationManager.
        
        Parameters
        ----------
        completion_request : CompletionRequest
            The CompletionRequest is the metadata object containing the details needed for
            the OrchestrationManager to assemble an agent with a language model and data source.
        context : Context
            The user context under which to execution completion requests.
        """
        self.completion_request = completion_request
        self.config = configuration
        self.llm = self.__get_llm(language_model=completion_request.language_model)
        self.agent = self.__create_agent(completion_request=completion_request, context=context)

    def __create_agent(self, completion_request: CompletionRequest, context: Context) -> AgentBase:
        """Creates an agent for executing completion requests."""
        agent = AgentFactory(completion_request=completion_request,
                             llm=self.llm, config=self.config, context=context)
        return agent.get_agent()

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
