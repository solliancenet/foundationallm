from foundationallm.hubs import Repository
from foundationallm.hubs.prompt import PromptMetadata
from foundationallm.hubs.prompt import PromptStorageManager

class PromptRepository(Repository):
    """The PromptRepository class is responsible for fetching available metadata values."""

    def get_metadata_values(self):
        mgr = PromptStorageManager()
        prompt_files = mgr.list_blobs()
        return [PromptMetadata(name=prompt_file.split('.')[0], prompt=mgr.read_file_content(prompt_file)) for prompt_file in prompt_files]
