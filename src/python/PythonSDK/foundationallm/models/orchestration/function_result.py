from typing import Optional
from pydantic import BaseModel, Field
from foundationallm.models.constants import AgentCapabilityCategories

class FunctionResult(BaseModel):
    """
    FunctionResult class encapsulates the input and output of function calls.
    """
    function_name: str = Field(None, description="The name of the function.")
    function_input: Optional[dict] = Field(None, description="The input to the function.")
    function_output: Optional[dict] = Field(None, description="The output of the function.")
    agent_capability_category: Optional[AgentCapabilityCategories] = Field(None, description="The category of capability assigned to the agent.")

    class Config:
        use_enum_values = True
        populate_by_name = True
        extra = "forbid"
