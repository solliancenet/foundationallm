from typing import List
from foundationallm.hubs import Metadata

class AgentMetadata(Metadata):
    name: str
    description: str
    allowed_data_source_names: List[str]