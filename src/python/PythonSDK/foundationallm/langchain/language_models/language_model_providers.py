from enum import Enum

class LanguageModelProviders(str, Enum):
    """Enumerator with the Language Model providers."""

    MICROSOFT = "microsoft"
    OPENAI = "openai"