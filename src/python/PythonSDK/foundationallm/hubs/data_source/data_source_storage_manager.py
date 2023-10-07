from foundationallm.config import Configuration
from foundationallm.storage import BlobStorageManager
from typing import List

class DataSourceStorageManager(BlobStorageManager):
     """The DataSourceStorageManager class is responsible for fetching available datasource values from Azure Blob Storage."""
     def __init__(self, container_name="data-sources", root_path=""):
         connection_string = Configuration().get_value("fllm-storage-connection-string")
         super().__init__(blob_connection_string=connection_string,
                             container_name=container_name, root_path=root_path)
         
     def read_file_content(self, path) -> str:
          return super().read_file_content(path).decode()
     
     def list_blobs(self):
          blob_list: List[dict] = list(super().list_blobs(path=""))
          blob_names = [blob["name"].split('/')[-1] for blob in blob_list]
          return blob_names