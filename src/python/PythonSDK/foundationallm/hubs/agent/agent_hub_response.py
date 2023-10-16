from pydantic import BaseModel
from foundationallm.hubs.agent import AgentMetadata

class AgentHubResponse(BaseModel):
    agent: AgentMetadata