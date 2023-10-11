from pydantic import BaseModel

from foundationallm.models.metadata import LanguageModelMetadata

class AgentMetadata(BaseModel):
    name: str
    type: str
    description: str
    prompt_template: str
    language_model: LanguageModelMetadata