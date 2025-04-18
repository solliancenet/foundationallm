from enum import Enum

class AzureAIResourceTypeNames(str, Enum):
    """The names of the resource types managed by the FoundationaLLM.AIModel resource provider."""
    PROJECTS = 'projects'
