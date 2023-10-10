from foundationallm.hubs import Repository
from foundationallm.hubs.prompt import PromptMetadata
from foundationallm.hubs.prompt import PromptHubStorageManager
from typing import List

class PromptRepository(Repository):
    """The PromptRepository class is responsible for fetching available metadata values."""

    def get_metadata_values(self) -> List[PromptMetadata]:
        mgr = PromptHubStorageManager(config=self.config)
        prompt_files = mgr.list_blobs(path="")
        # Prepare the name so that it adheres to the prompt namespace, eg. AgentName.PromptName (AgentName is also the folder, PromptName is the file name without extension)
        return [PromptMetadata(name=prompt_file.split('.')[0].replace('/','.'), prompt=mgr.read_file_content(prompt_file)) for prompt_file in prompt_files]
