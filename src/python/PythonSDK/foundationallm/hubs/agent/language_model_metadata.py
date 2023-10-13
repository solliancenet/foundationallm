from numpy.random import f
from foundationallm.hubs import Metadata
from foundationallm.hubs.agent import LanguageModelType
from foundationallm.hubs.agent import LanguageModelProvider

class LanguageModelMetadata(Metadata):
    model_type: LanguageModelType
    provider: LanguageModelProvider
    temperature: float = 0.0
    use_chat: bool = True