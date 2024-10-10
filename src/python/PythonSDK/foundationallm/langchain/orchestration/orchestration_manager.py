from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.agents import AgentFactory, LangChainAgentBase
from foundationallm.operations import OperationsManager
from foundationallm.models.orchestration import (
    CompletionRequestBase,
    CompletionResponse
)

class OrchestrationManager:
    """Client that acts as the entry point for interacting with the FoundationaLLM Python SDK."""

    def __init__(self,
        completion_request: CompletionRequestBase,
        instance_id: str,
        user_identity: UserIdentity,
        configuration: Configuration,
        operations_manager: OperationsManager):
        """
        Initializes an instance of the OrchestrationManager.
        
        Parameters
        ----------
        completion_request : CompletionRequest
            The CompletionRequest is the metadata object containing the details needed for
            the OrchestrationManager to assemble an agent.
        instance_id : str
            The unique identifier of the FoundationaLLM instance.
        user_identity : UserIdentity
            The user context under which to execution completion requests.
        configuration : Configuration
            The configuration object containing the details needed for the OrchestrationManager to assemble an agent.
        operations_manager : OperationsManager
            The operations manager object for allowing an agent to interact with the State API.
        """
        self.agent = self.__create_agent(
            completion_request = completion_request,
            config = configuration,
            operations_manager = operations_manager,
            instance_id = instance_id,
            user_identity = user_identity
        )

    def __create_agent(self,
                       completion_request: CompletionRequestBase,
                       config: Configuration,
                       operations_manager: OperationsManager,
                       instance_id: str,
                       user_identity: UserIdentity) -> LangChainAgentBase:
        """Creates an agent for executing completion requests."""
        return AgentFactory().get_agent(completion_request.agent.type, config, operations_manager, instance_id, user_identity)

    def invoke(self, request: CompletionRequestBase) -> CompletionResponse:
        """
        Executes a completion request against the LanguageModel using 
        the LangChain agent assembled by the OrchestrationManager.
        
        Parameters
        ----------
        request : CompletionRequestBase
            The completion request to execute.
            
        Returns
        -------
        CompletionResponse
            Object containing the completion response and token usage details.
        """
        return self.agent.invoke(request)

    async def ainvoke(self, request: CompletionRequestBase) -> CompletionResponse:
        """
        Executes an async completion request against the LanguageModel using 
        the LangChain agent assembled by the OrchestrationManager.
        
        Parameters
        ----------
        request : CompletionRequestBase
            The completion request to execute.
            
        Returns
        -------
        CompletionResponse
            Object containing the completion response and token usage details.
        """        
        completion_response = await self.agent.ainvoke(request)
        return completion_response
