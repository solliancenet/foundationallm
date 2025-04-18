from enum import Enum

class ResourceProviderNames(str, Enum):
    """The names of the FoundationaLLM resource providers."""
    FOUNDATIONALLM_AIMODEL = 'FoundationaLLM.AIModel'
    FOUNDATIONALLM_AZUREAI = 'FoundationaLLM.AzureAI'
    FOUNDATIONALLM_PROMPT = 'FoundationaLLM.Prompt'
    FOUNDATIONALLM_VECTORIZATION = 'FoundationaLLM.Vectorization'
