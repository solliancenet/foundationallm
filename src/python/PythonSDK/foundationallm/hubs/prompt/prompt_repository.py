from foundationallm.hubs import Repository
from foundationallm.hubs.prompt import PromptMetadata

class PromptRepository(Repository):
    """The PromptRepository class is responsible for fetching available metadata values."""

    def get_metadata_values(self):
        # This should connect to the data source and fetch metadata values
        # For simplicity, returning a static list
        return [
            PromptMetadata("DefaultAgent", "Your name is Solli and you are here to assist employees with their questions."),
            PromptMetadata("Coco", "Your name is Coco, and you can answer questions about past weather reports.")
        ]