from abc import ABC, abstractmethod
from foundationallm.config import Configuration
from foundationallm.hubs import Metadata
from typing import List

class Repository(ABC):
    """The Repository class is responsible for fetching available metadata values."""

    def __init__(self, config: Configuration):
        self.config = config    

    @abstractmethod
    def get_metadata_values(self) -> List[Metadata]:
        pass