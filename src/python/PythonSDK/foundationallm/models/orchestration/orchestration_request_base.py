from pydantic import BaseModel

class OrchestrationRequestBase(BaseModel):
    user_prompt: str