from pydantic import BaseModel
from typing import Union

class CompletionResponse(BaseModel):
    completion: Union[str, set]
    user_prompt: str
    prompt_tokens: int = 0
    completion_tokens: int = 0
    total_tokens: int = 0
    total_cost: float = 0.0
    #user_prompt_embedding: list(float) = None