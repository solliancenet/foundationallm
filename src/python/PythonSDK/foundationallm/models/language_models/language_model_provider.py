from enum import Enum

class LanguageModelProvider(str, Enum):
    """Enumerator of the Language Model providers."""

    MICROSOFT = "microsoft"
    OPENAI = "openai"
    BEDROCK = "bedrock"
