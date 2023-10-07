from typing import List, Optional
from foundationallm.hubs import Metadata

class AgentMetadata(Metadata):
    """Class representing the metadata for an agent."""
    name: str
    description: str
    allowed_data_source_names: Optional[List[str]] = None