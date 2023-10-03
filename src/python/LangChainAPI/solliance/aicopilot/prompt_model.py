from pydantic import BaseModel

class PromptModel(BaseModel):
    prompt: str