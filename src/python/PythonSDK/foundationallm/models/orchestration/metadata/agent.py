from .metadata_base import MetadataBase

class Agent(MetadataBase):
    """Agent metadata model."""
    prompt_template: str
    type: str