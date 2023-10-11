from numpy.random import f
from foundationallm.hubs import Metadata
from foundationallm.hubs.agent import LanguageModelType
from foundationallm.hubs.agent import LanguageModelSubtype
from foundationallm.hubs.agent import LanguageModelProvider

class LanguageModelMetadata(Metadata):
    model_type: LanguageModelType
    model_subtype: LanguageModelSubtype
    provider: LanguageModelProvider
    temperature: float = 0.0