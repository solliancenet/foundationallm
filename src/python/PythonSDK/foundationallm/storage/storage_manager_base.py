import abc


class StorageManagerBase(metaclass=abc.ABCMeta):
    """ The StorageManagerBase class is responsible for defining how to interact with a storage account."""
    @abc.abstractmethod
    def file_exists(self, path) -> bool:
        pass

    @abc.abstractmethod
    def read_file_content(self, path) -> bytes:
        pass

    @abc.abstractmethod
    def write_file_content(self, path, content):
        pass

    @abc.abstractmethod
    def delete_file(self, path):
        pass
