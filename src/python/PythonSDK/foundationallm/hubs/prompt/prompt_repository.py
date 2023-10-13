from foundationallm.hubs import Repository
from foundationallm.hubs.prompt import PromptMetadata
from foundationallm.hubs.prompt import PromptHubStorageManager
from typing import List

class PromptRepository(Repository):
    """The PromptRepository class is responsible for fetching available metadata values."""

    def get_metadata_values(self, pattern:str=None) -> List[PromptMetadata]:
        """
        Returns a list of PromptMetadata objects, optionally filtered by a pattern.
        
        Background: An Agent may have many prompts. In storage, they are located in AgentName/PromptName1.txt, AgentName/PromptName2.txt, etc.
        The PromptMetadata name represents the path as a namespace ex. AgentName.PromptName1, AgentName.PromptName2, etc.
        
        Args:
        pattern (str): The Agent name defines the pattern/location of the prompts to return, if None, return all prompts.
        """
        mgr = PromptHubStorageManager(config=self.config)
        if pattern is None:
            pattern = ""        
        prompt_files = mgr.list_blobs(path=pattern)
        # Prepare the name so that it adheres to the prompt namespace, eg. AgentName.PromptName (AgentName is also the folder, PromptName is the file name without extension)
        return [PromptMetadata(name=prompt_file.split('.')[0].replace('/','.'), prompt=mgr.read_file_content(prompt_file)) for prompt_file in prompt_files]
    
    def get_metadata_by_name(self, name: str) -> PromptMetadata:
        """
        Returns a PromptMetadata object by name.
        
        Args:
        name (str): The name of the prompt to return, in the format of AgentName.PromptName
        """
        mgr = PromptHubStorageManager(config=self.config)
        prompt_file = mgr.read_file_content(name.replace('.', '/') + '.txt')
        return PromptMetadata(name=name, prompt=prompt_file)

