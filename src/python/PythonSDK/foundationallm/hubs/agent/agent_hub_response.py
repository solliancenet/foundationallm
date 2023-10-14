from pydantic import BaseModel
from typing import List
from foundationallm.hubs.agent import AgentMetadata

class AgentHubResponse(BaseModel):
    agents: List[AgentMetadata]