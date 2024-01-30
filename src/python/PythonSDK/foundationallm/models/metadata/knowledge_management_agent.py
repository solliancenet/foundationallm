"""
Class Name: KnowledgeManagementAgent

Description: Encapsulates the metadata for the agent
fulfilling the orchestration request.
"""
from typing import Optional
from foundationallm.models.language_models import LanguageModel
from .agent_base import AgentBase

class KnowledgeManagementAgent(AgentBase):
    """Knowlege Management Agent metadata model."""
    indexing_profile: Optional[str] = None
    embedding_profile: Optional[str] = None

