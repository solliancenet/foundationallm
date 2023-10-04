from pydantic import BaseModel
from typing import List, Optional
from foundationallm.models import Message

class Session(BaseModel):    
    user_prompt: str
    message_history: Optional(List[Message])
    user_context: str