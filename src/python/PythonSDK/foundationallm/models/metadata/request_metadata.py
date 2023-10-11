from pydantic import BaseModel

from .agent_metadata import AgentMetadata
from .data_source_metadata import DataSourceMetadata

class RequestMetadata(BaseModel):
    agent: AgentMetadata
    data_source: DataSourceMetadata