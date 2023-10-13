from enum import Enum

class LanguageModelProvider(str, Enum):
    MICROSOFT = "microsoft"
    OPENAI = "openai"