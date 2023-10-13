from pydantic import BaseModel, confloat
from typing import Annotated

from foundationallm.langchain.language_models import LanguageModelTypes, LanguageModelProviders

class LanguageModel(BaseModel):
    """Language model metadata model."""
    type: str = LanguageModelTypes.OPENAI
    provider: str = LanguageModelProviders.MICROSOFT
    temperature: Annotated[float, confloat(ge=0, le=1)] = 0
    use_chat: bool = True