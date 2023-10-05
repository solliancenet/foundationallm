from foundationallm.hubs import Repository
from foundationallm.hubs.agent import AgentMetadata
from typing import List

class AgentRepository(Repository):
    def get_metadata_values(self) -> List[AgentMetadata]:
        # This should connect to the data source and fetch metadata values
        # For simplicity, returning a static list
        agent_list = [
            AgentMetadata(name="DefaultAgent", description="The default agent for the system.", allowed_data_source_names=[]),
            AgentMetadata(name="Coco", description="Agent that has access to CocoRahs weather data.", allowed_data_source_names=["cocorahs"])
        ]
        
        return agent_list