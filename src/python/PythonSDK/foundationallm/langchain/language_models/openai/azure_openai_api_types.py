from enum import Enum

class AzureOpenAIAPIType(str, Enum):
    """Enumerator with the Agent Providers."""

    AZURE = "azure"
    AZURE_ACTIVE_DIRECTORY_AUTHENTICATION = "azure_ad"