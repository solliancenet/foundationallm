from pydantic import BaseModel

class OrchestrationRequest(BaseModel):
    """Base orchestration request."""
    user_prompt: str