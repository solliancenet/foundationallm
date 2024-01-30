from typing import Optional
from .metadata_base import MetadataBase

class Agent(MetadataBase):
    """Agent metadata model."""
    prompt_prefix: Optional[str] = None
    prompt_suffix: Optional[str] = None
    
