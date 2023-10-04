from abc import ABC, abstractmethod
from foundationallm.hubs import Metadata
from typing import List

class Resolver(ABC):
    """The Resolver class is responsible for resolving a request to a list of metadata value."""
    
    @abstractmethod
    def resolve(self, request, metadata_values) -> List[Metadata]:
        pass