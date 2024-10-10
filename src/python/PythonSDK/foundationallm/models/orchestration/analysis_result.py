"""
Class: AnalysisResult
Description: AnalysisResult class encapsulates the input and output of tool calls.
"""
from typing import Optional
from pydantic import BaseModel, Field
from foundationallm.models.constants import AgentCapabilityCategories

class AnalysisResult(BaseModel):
    """AnalysisResult class encapsulates the input and output of tool calls."""

    tool_name:str = Field(None, description="The name of the tool.")
    tool_input: Optional[str] = Field("", description="The input to the tool.")
    tool_output: Optional[str] = Field("", description="The output of the tool.")
    agent_capability_category: Optional[AgentCapabilityCategories] = Field(None, description="The category of capability assigned to the agent.")

    class Config:
        use_enum_values = True
        populate_by_name = True
        extra = "forbid"
