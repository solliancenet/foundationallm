from abc import ABC, abstractmethod

from foundationallm.models.orchestration import OrchestrationResponseBase

class AgentBase(ABC):
    """Abstract base class for Agents"""
    
    @abstractmethod
    def run(self) -> OrchestrationResponseBase:
        """agent run"""