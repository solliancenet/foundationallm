"""
Classes:
    AzureAISearchSettings: Settings for an Azure AI Search indexing profile.
    AzureAISearchConfigurationReferences: Configuration references for an Azure AI Search indexing profile.
    AzureAISearchIndexingProfile: An Azure AI Search indexing profile.
Description:
    Settings specific to the Azure AI Search Indexes
"""
from typing import Any, Self
from foundationallm.models.resource_providers.vectorization import IndexingProfileBase
from .azure_ai_search_settings import AzureAISearchSettings
from .azure_ai_search_configuration_references import AzureAISearchConfigurationReferences
from foundationallm.utils import ObjectUtils
from foundationallm.langchain.exceptions import LangChainException

class AzureAISearchIndexingProfile(IndexingProfileBase):
    """
    An Azure AI Search indexing profile.
    """
    settings: AzureAISearchSettings
    configuration_references: AzureAISearchConfigurationReferences
    
    @staticmethod
    def from_object(obj: Any) -> Self:

        indexing_profile: AzureAISearchIndexingProfile = None
        
        try:
            indexing_profile = AzureAISearchIndexingProfile(**ObjectUtils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"The indexing profile object provided in the agent parameters is invalid. {str(e)}", 400)
        
        if indexing_profile is None:
            raise LangChainException("The indexing object is missing in the agent parameters.", 400)

        return indexing_profile
