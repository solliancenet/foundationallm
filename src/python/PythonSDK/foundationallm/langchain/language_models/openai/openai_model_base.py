from pydantic import confloat
from typing import Annotated

from foundationallm.langchain.language_models import LanguageModelBase

class OpenAIModelBase(LanguageModelBase):
    """OpenAI model base class."""
    openai_api_base = str
    openai_api_key: str
    openai_organization: str = ''
    max_retries: int = 6
    verbose: bool = False
    temperature: Annotated[float, confloat(ge=0.0, le=1.0)] = 0
    