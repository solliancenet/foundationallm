from pydantic import BaseModel
from typing import Optional

class AgentHubRequest(BaseModel):
    """     
    AgentHubRequest contains the information needed to retrieve AgentMetadata from the AgentHub.
    
    agent_name: the name of the agent to retrieve, send an empty string or None to retrieve all prompts.
        
    """
    agent_name: Optional[str] = None