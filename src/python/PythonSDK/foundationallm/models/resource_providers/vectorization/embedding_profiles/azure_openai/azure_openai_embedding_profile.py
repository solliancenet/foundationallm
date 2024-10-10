"""
Classes:   
    AzureAISearchIndexingProfile: An Azure AI Search indexing profile.
Description:
    Settings specific to the Azure AI Search Indexes
"""
from typing import Any, Self, Optional
from foundationallm.models.resource_providers.vectorization import EmbeddingProfileBase
from foundationallm.utils import ObjectUtils
from foundationallm.langchain.exceptions import LangChainException

class AzureOpenAIEmbeddingProfile(EmbeddingProfileBase):
    """
    An Azure AI Search indexing profile.
    """
    settings: Optional[dict] = None
    configuration_references: Optional[dict] = None
    
    @staticmethod
    def from_object(obj: Any) -> Self:

        text_embedding_profile: AzureOpenAIEmbeddingProfile = None

        try:
            text_embedding_profile = AzureOpenAIEmbeddingProfile(**ObjectUtils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"The text embedding profile object provided in the agent parameters is invalid. {str(e)}", 400)
        
        if text_embedding_profile is None:
            raise LangChainException("The text embedding profile object is missing in the agent parameters.", 400)

        return text_embedding_profile
