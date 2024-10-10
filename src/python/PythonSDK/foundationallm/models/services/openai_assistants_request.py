"""
Encapsulates properties useful for calling the OpenAI Assistants API.
"""
from pydantic import BaseModel
from typing import List, Optional

class OpenAIAssistantsAPIRequest(BaseModel):
    """
    Encapsulates properties useful for calling the OpenAI Assistants API.
        assistant_id: str - The ID of the assistant to use.
        thread_id: str - The ID of the conversation thread to use.
        attachments: Optional[List[AttachmentProperties]] - The list of OpenAI file attachments to include in the request.
        user_prompt: str - The user prompt/message to send to the assistants API.
    """
    document_id: Optional[str] = None
    operation_id: str
    instance_id: str
    assistant_id: str
    thread_id: str
    attachments: Optional[List[str]] = []
    user_prompt: str
