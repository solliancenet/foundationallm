from pydantic import BaseModel
from typing import List
from foundationallm.hubs.prompt import PromptMetadata

class prompt_hub_response(BaseModel):
    prompts: List[PromptMetadata]