from typing import List, Optional
from pydantic import BaseModel
from foundationallm.models.orchestration import MessageHistoryItem

class CompletionRequestBase(BaseModel):
    """
    Orchestration completion request.
    """
    user_prompt: str    
    message_history: Optional[List[MessageHistoryItem]] = []
