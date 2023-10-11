from pydantic import BaseModel
from pydantic import confloat
from typing import Annotated

class LanguageModelMetadata(BaseModel):
    type: str
    subtype: str
    provider: str = 'azure'
    temperature: Annotated[float, confloat(ge=0, le=1)] = 0