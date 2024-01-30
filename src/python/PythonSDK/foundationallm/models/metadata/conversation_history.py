"""
Class Name: ConversationHistory

Description: Encapsulates the metadata for the
conversation history metadata of an agent.
"""
from typing import Optional
from pydantic import BaseModel

class ConversationHistory(BaseModel):
    """ Conversation History metadata model."""
    enabled: Optional[bool] = False
    max_history: Optional[int] = 10
