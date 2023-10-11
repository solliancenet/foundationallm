from pydantic import BaseModel

from foundationallm.models.metadata import AgentMetadata

class OrchestrationRequestBase(BaseModel):
    user_prompt: str
    agent: AgentMetadata