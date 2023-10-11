from pydantic import BaseModel

class OrchestrationResponseBase(BaseModel):
    user_prompt: str
    prompt_tokens: int = 0
    completion_tokens: int = 0
    total_tokens: int = 0
    total_cost: float = 0.0