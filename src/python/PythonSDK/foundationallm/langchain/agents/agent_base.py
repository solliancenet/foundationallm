from abc import ABC, abstractmethod

from foundationallm.models.orchestration import OrchestrationResponse

class AgentBase(ABC):
    """Abstract base class for Agents"""
    
    @abstractmethod
    def run(self) -> OrchestrationResponse:
        """
        Execute the agent's run method

        Returns
        -------
        OrchestrationResponse
            Returns a response containing the completion plus token usage and cost details.
        """