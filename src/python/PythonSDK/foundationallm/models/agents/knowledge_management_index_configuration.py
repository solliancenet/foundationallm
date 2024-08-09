"""
Encapsulates knowledge management agent index information.
"""
from typing import Optional
from pydantic import BaseModel
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.models.resource_providers.vectorization import AzureAISearchIndexingProfile

class KnowledgeManagementIndexConfiguration(BaseModel):
    """Knowlege Management Agent metadata model."""
    indexing_profile : AzureAISearchIndexingProfile
    api_endpoint_configuration: APIEndpointConfiguration
