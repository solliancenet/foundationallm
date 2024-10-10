from foundationallm.config import Configuration, UserIdentity
from foundationallm.operations import OperationsManager
from foundationallm.langchain.agents import (
    LangChainAgentBase,
    LangChainKnowledgeManagementAgent
)

class AgentFactory:
    """
    Factory to determine which agent to use.
    """
    def get_agent(
        self,
        agent_type: str,
        config: Configuration,
        operations_manager: OperationsManager,
        instance_id: str,
        user_identity: UserIdentity) -> LangChainAgentBase:
        """
        Retrieves an agent of the the requested type.

        Parameters
        ----------
        agent_type : str
            The type type assign to the agent returned.
        config : Configuration
            The configuration object containing the details needed for the OrchestrationManager to assemble an agent.
        operations_manager : OperationsManager
            The operations manager object for allowing an agent to interact with the State API.
        instance_id : str
            The unique identifier of the FoundationaLLM instance.
        user_identity : UserIdentity
            The user context under which to execution completion requests.

        Returns
        -------
        AgentBase
            Returns an agent of the requested type.
        """
        if agent_type is None:
            raise ValueError("Agent not constructed. Cannot access an object of 'NoneType'.")
        match agent_type:
            case 'knowledge-management':                
                return LangChainKnowledgeManagementAgent(
                        instance_id=instance_id,
                        user_identity=user_identity,
                        config=config,
                        operations_manager=operations_manager)
            case _:
                raise ValueError(f'The agent type {agent_type} is not supported.')
