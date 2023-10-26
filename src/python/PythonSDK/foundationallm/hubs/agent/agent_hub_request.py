from pydantic import BaseModel
from typing import Optional
from foundationallm.models.orchestration import MessageHistoryItem

class AgentHubRequest(BaseModel):
    """     
    AgentHubRequest contains the information needed to retrieve AgentMetadata from the AgentHub.
    
    user_prompt: the prompt the user entered into the system.
    message_history: the current conversation history with the user.
        
    """
    user_prompt:str
    message_history: Optional[MessageHistoryItem] = None