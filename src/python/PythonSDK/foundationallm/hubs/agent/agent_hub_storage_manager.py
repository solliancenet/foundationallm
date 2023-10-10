from foundationallm.config import Configuration
from foundationallm.storage import BlobStorageManager
from typing import List

class AgentHubStorageManager(BlobStorageManager):
     """The AgentHubStorageManager class is responsible for fetching available agent values from Azure Blob Storage."""
     def __init__(self, config: Configuration = None):
         connection_string = config.get_value("foundationallm-storage-connection-string")
         container_name = config.get_value("foundationallm-agent-metadata-storage-container")
         super().__init__(blob_connection_string=connection_string,
                             container_name=container_name)
         
     def read_file_content(self, path) -> str:
          return super().read_file_content(path).decode()
     
     def list_blobs(self):
          blob_list: List[dict] = list(super().list_blobs(path=""))
          blob_names = [blob["name"].split('/')[-1] for blob in blob_list]
          return blob_names