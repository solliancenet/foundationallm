"""
Class Name: Gatekeeper

Description: Encapsulates the metadata for the
gatekeeper metadata of an agent.
"""
from typing import Optional, List
from pydantic import BaseModel

class Gatekeeper(BaseModel):
    """ Gatekeeper metadata model."""
    use_system_setting: Optional[bool] = True
    options:Optional[List[str]] = None
