from pydantic import BaseModel

class RequestBase(BaseModel):
    prompt: str