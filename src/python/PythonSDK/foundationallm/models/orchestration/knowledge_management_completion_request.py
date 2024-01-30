"""
Class Name: KnowledgeManagementCompletionRequest
Description: Encapsulates the metadata required to complete a knowledge management orchestration request.
"""
from typing import Optional
from foundationallm.models.metadata import KnowledgeManagementAgent
from .completion_request_base import CompletionRequestBase

class KnowledgeManagementCompletionRequest(CompletionRequestBase):
    """
    Orchestration completion request.
    """    
    agent: Optional[KnowledgeManagementAgent] = None
