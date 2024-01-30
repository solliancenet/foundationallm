from typing import Optional
from pydantic import BaseModel
from foundationallm.models.language_models import LanguageModelType, LanguageModelProvider

class EmbeddingModel(BaseModel):
    """Embedding model metadata model."""
    type: str = LanguageModelType.OPENAI
    provider: Optional[str] = LanguageModelProvider.MICROSOFT
    deployment: Optional[str] = "FoundationaLLM:AzureOpenAI:API:Embeddings:DeploymentName"    
    chunk_size: Optional[int] = 1000
    api_endpoint: Optional[str] = "FoundationaLLM:AzureOpenAI:API:Endpoint"
    api_key: Optional[str] = "FoundationaLLM:AzureOpenAI:API:Key"
    api_version: Optional[str] = "FoundationaLLM:AzureOpenAI:API:Version"
