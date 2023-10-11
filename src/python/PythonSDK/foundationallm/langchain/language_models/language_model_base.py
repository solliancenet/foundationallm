from abc import ABC, abstractmethod
from pydantic import confloat
from typing import Annotated

from langchain.base_language import BaseLanguageModel
from foundationallm.config import Configuration

class LanguageModelBase(ABC):
    temperature: Annotated[float, confloat(ge=0.0, le=1.0)] = 0

    def __init__(self, app_config: Configuration):
        self.app_config = app_config

    @abstractmethod
    def get_language_model(self) -> BaseLanguageModel:
        """Retrieve language model"""
    