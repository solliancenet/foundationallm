"""
Encapsulates the metadata for the agent fulfilling the orchestration request.
"""
from typing import Optional
from foundationallm.models.agents import AgentBase, AgentVectorizationSettings

class KnowledgeManagementAgent(AgentBase):
    """Knowlege Management Agent metadata model."""
    vectorization: Optional[AgentVectorizationSettings] = None

