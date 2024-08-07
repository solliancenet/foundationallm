from typing import Optional
from pydantic import BaseModel
from foundationallm.models.language_models import LanguageModelType, LanguageModelProvider

class EmbeddingModel(BaseModel):
    """Embedding model metadata model."""
    type: str = LanguageModelType.OPENAI
    provider: Optional[str] = LanguageModelProvider.MICROSOFT
    deployment: Optional[str] = None
    chunk_size: Optional[int] = 500
    api_endpoint: Optional[str] = None
    api_key: Optional[str] = None
    api_version: Optional[str] = None
