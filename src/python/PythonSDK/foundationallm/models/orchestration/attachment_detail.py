from pydantic import BaseModel, Field
from typing import Optional

class AttachmentDetail(BaseModel):
    """
    Represents an attachment in a chat message.
    """
    object_id: Optional[str] = Field(None, alias='objectId')
    display_name: Optional[str] = Field(None, alias='displayName')
    content_type: Optional[str] = Field(None, alias='contentType')
