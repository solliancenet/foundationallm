from abc import ABC, abstractmethod
from foundationallm.hubs import Metadata
from typing import List

class Repository(ABC):
    """The Repository class is responsible for fetching available metadata values."""
    
    @abstractmethod
    def get_metadata_values(self) -> List[Metadata]:
        pass