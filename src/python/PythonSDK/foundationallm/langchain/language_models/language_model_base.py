from abc import ABC, abstractmethod

from langchain.base_language import BaseLanguageModel
from foundationallm.config import Configuration

class LanguageModelBase(ABC):
    """Abstract base class for language models."""
    
    def __init__(self, config: Configuration):
        """
        Initializer
        
        Parameters
        ----------
        app_config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.config = config

    @abstractmethod
    def get_language_model(self) -> BaseLanguageModel:
        """
        Retrieve language model
        
        Returns
        -------
        BaseLanguageModel
            The language model to use.
        """
    