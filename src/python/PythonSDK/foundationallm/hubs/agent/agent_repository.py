from foundationallm.hubs import Repository
from foundationallm.hubs.agent import AgentMetadata
from typing import List

class AgentRepository(Repository):
    def get_metadata_values(self) -> List[AgentMetadata]:
        # This should connect to the data source and fetch metadata values
        # For simplicity, returning a static list
        agent_list = [
            AgentMetadata("DefaultAgent", "The default agent for the system.", None),
            AgentMetadata("Coco", "Agent that has access to CocoRahs weather data.", ["cocorahs"])
        ]
        
        return agent_list