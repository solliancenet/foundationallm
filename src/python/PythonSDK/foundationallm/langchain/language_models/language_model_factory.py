from langchain_core.language_models import BaseLanguageModel

from foundationallm.config import Configuration
from foundationallm.models.language_models import LanguageModelType, LanguageModel
from foundationallm.langchain.language_models.openai import OpenAIModel

class LanguageModelFactory:
    """
    Factory class for determine which language models to use.
    """
    def __init__(self, language_model: LanguageModel, config: Configuration):
        self.config = config
        self.language_model = language_model

    def get_llm(self) -> BaseLanguageModel:
        """
        Retrieves the language model to use for completion and embedding requests.
        
        Returns
        -------
        BaseLanguageModel
            Returns the language model to use for completion and embedding requests.
        """
        match self.language_model.type:
            case LanguageModelType.OPENAI:
                return OpenAIModel(config = self.config)
            case _:
                # Default to OpenAI model.
                return OpenAIModel(config = self.config)
