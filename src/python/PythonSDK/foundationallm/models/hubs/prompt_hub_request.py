from pydantic import BaseModel

class prompt_hub_request(BaseModel):
    agent_name: str