from pydantic import BaseModel
from typing import Optional

class PromptHubRequest(BaseModel):
    """     
    PromptHubRequest contains the information needed to retrieve prompts from the PromptHub.
    
    agent_name: the name of the agent to retrieve prompts for, send an empty string or None to retrieve all prompts.
        
    """
    agent_name: Optional[str] = None