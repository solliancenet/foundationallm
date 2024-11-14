"""
Encapsulates properties an agent tool
"""
from typing import Optional
from pydantic import BaseModel, Field

class AgentTool(BaseModel):
    """
    Encapsulates properties for an agent tool.
    """
    name: str = Field(..., description="The name of the agent tool.")
    description: str = Field(..., description="The description of the agent tool.")
    ai_model_object_ids: Optional[dict[str, str]] = Field(default=[], description="A dictionary of object identifiers of the AIModel objects for the agent tool.")
    api_endpoint_configuration_object_ids: Optional[dict[str, str]] = Field(default=[], description="A dictionary of object identifiers of the APIEndpointConfiguration objects for the agent tool.")
    properties: Optional[dict[str, object]] = Field(default=[], description="A dictionary of properties for the agent tool.")
