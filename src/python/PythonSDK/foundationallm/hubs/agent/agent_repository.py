from foundationallm.config import Configuration
from foundationallm.hubs import Repository
from foundationallm.hubs.agent import AgentMetadata, AgentHubStorageManager

from typing import List

class AgentRepository(Repository): 
    """ The AgentRepository is responsible for retrieving data source metadata from storage."""
    
    def get_metadata_values(self) -> List[AgentMetadata]:
        mgr = AgentHubStorageManager(config=self.config)
        agent_files = mgr.list_blobs()
        return [AgentMetadata.model_validate_json(mgr.read_file_content(agent_file)) for agent_file in agent_files]