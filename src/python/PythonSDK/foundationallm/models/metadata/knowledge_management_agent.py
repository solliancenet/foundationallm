"""
Class Name: KnowledgeManagementAgent

Description:
    Encapsulates the metadata for the agent
    fulfilling the orchestration request.
"""
from typing import Optional, List
from pydantic import validator
from .agent_base import AgentBase

class KnowledgeManagementAgent(AgentBase):
    """Knowlege Management Agent metadata model."""
    indexing_profile_object_ids: Optional[List[str]] = None
    text_embedding_profile_object_id: Optional[str] = None

