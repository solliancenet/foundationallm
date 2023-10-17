from foundationallm.config import Configuration
from foundationallm.hubs import Repository
from foundationallm.hubs.agent import AgentMetadata, AgentHubStorageManager

from typing import List

class AgentRepository(Repository): 
    """ The AgentRepository is responsible for retrieving data source metadata from storage."""
    
    def get_metadata_values(self, pattern:str=None) -> List[AgentMetadata]:
        """
        Returns a list of AgentMetadata objects, optionally filtered by a pattern.
        
        Background: An Agent may have many prompts. In storage, they are located in AgentName/PromptName1.txt, AgentName/PromptName2.txt, etc.
        The PromptMetadata name represents the path as a namespace ex. AgentName.PromptName1, AgentName.PromptName2, etc.
        
        Args:
        pattern (str): The Agent name to return, if None or empty, return all Agents.
        """
        mgr = AgentHubStorageManager(config=self.config)
        if pattern is None:
            pattern = ""
        agent_files = mgr.list_blobs(path=pattern)
        return [AgentMetadata.model_validate_json(mgr.read_file_content(agent_file)) for agent_file in agent_files]
    
    def get_metadata_by_name(self, name: str) -> AgentMetadata: 
        mgr = AgentHubStorageManager(config=self.config)
        agent_file = name + ".json"
        return AgentMetadata.model_validate_json(mgr.read_file_content(agent_file))
