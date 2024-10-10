from datetime import datetime
from pydantic import BaseModel, Field
from typing import Optional
from foundationallm.models.orchestration import CompletionResponse

class LongRunningOperation(BaseModel):
    """
    Class representing a long running operation.
    """
    operation_id: str = Field(description='The unique identifier for the operation.')
    status: str = Field(description='The status of the operation.')
    status_message: Optional[str] = Field(description='The message associated with the operation status.')
    last_updated: Optional[datetime] = Field(default=None, description='The timestamp of the last update to the operation.')
    result: Optional[CompletionResponse] = Field(default=None, description='The result of the operation.')
