from pydantic import BaseModel

class OrchestrationResponse(BaseModel):
    """Orchestration response base class."""
    user_prompt: str
    prompt_tokens: int = 0
    completion_tokens: int = 0
    total_tokens: int = 0
    total_cost: float = 0.0