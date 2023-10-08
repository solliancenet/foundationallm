from pydantic import BaseModel

class CompletionResponse(BaseModel):
    completion: str
    user_prompt: str
    user_prompt_tokens: int = 0
    response_tokens: int = 0
    #user_prompt_embedding: list(float) = None