from abc import ABC, abstractmethod
class StorageManagerBase(ABC):
    """ The StorageManagerBase class is responsible for defining how to interact with a storage account."""
    @abstractmethod
    def file_exists(self, path) -> bool:
        pass

    @abstractmethod
    def read_file_content(self, path) -> bytes:
        pass

    @abstractmethod
    def write_file_content(self, path, content):
        pass

    @abstractmethod
    def delete_file(self, path):
        pass
