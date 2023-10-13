from pydantic import BaseModel
from typing import List
from foundationallm.hubs.prompt import PromptMetadata

class PromptHubResponse(BaseModel):
    prompts: List[PromptMetadata]