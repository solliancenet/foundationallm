from abc import ABC, abstractmethod

class Credential(ABC):
    """Abstract base class for Credential objects"""
    
    @abstractmethod
    def get_credential(self):
        """Retrieves credentials"""
