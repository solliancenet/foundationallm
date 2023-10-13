from enum import Enum

class LanguageModelType(str, Enum):
    """Enumerator with the Language Model types."""

    OPENAI = 'openai'