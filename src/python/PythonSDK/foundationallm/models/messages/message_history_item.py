from pydantic import BaseModel, Field
from typing import List, Optional

from foundationallm.models.orchestration import ContentArtifact, AttachmentDetail

class MessageHistoryItem(BaseModel):
    """
        Represents an historic message sender and text item.

        Parameters
        ----------
        sender : str
            The sender of the message (e.g., "Agent", "User")
        text : str
            The message text.
    """
    sender: str
    text: str
    content_artifacts : Optional[List[ContentArtifact]] = []
    attachments : Optional[List[AttachmentDetail]] = []
