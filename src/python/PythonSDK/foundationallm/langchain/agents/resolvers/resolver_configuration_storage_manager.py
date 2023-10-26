from foundationallm.config import Configuration
from foundationallm.storage import BlobStorageManager
from typing import List

class ResolverConfigurationStorageManager(BlobStorageManager):
     """The ResolverConfigurationStorageManager class is responsible for fetching built-in agent configuration details from Azure Blob Storage."""
     def __init__(self, config: Configuration = None):
         connection_string = config.get_value("FoundationaLLM:BlobStorage:ConnectionString")
         container_name = config.get_value("FoundationaLLM:SystemAgents:StorageContainer")
         
         super().__init__(blob_connection_string=connection_string,
                             container_name=container_name)
         
     def read_file_content(self, path) -> str:       
        return super().read_file_content(path).decode()     
   