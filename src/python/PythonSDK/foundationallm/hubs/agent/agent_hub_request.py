from pydantic import BaseModel
from typing import Optional

class AgentHubRequest(BaseModel):
    """     
    AgentHubRequest contains the information needed to retrieve AgentMetadata from the AgentHub.
    
    user_prompt: the prompt the user entered into the system.
    user_context: details about the user making the request.
        
    """
    user_prompt:str
    user_context: Optional[str] = None