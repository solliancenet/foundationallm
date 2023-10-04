from dataclasses import dataclass
from foundationallm.hubs import Metadata
from foundationallm.hubs.prompt import PromptType

@dataclass
class PromptMetadata(Metadata):
    """Class representing the metadata for a prompt"""

    name: str
    prompt_type: PromptType
    prompt: str