from foundationallm.storage import StorageManagerBase
from azure.storage.blob import BlobServiceClient
from io import BytesIO
from zipfile import ZIP_DEFLATED
import fnmatch


class BlobStorageManager(StorageManagerBase):
    """ The BlobStorageManager class is responsible for managing files in Azure Blob Storage."""

    def __init__(self, blob_connection_string=None, container_name=None):

        if blob_connection_string is None or blob_connection_string == '':
            raise ValueError(f'The blob_connection_string parameter must be set to a valid connection string.')
        if container_name is None or container_name == '':
            raise ValueError(f'The container_name parameter must be set to a valid container.')
      
        self.blob_connection_string = blob_connection_string
        self.container_name = container_name        
        self.blob_service_client = BlobServiceClient.from_connection_string(self.blob_connection_string)
        self.blob_container_client = self.blob_service_client.get_container_client(container_name)
        

    def __get_full_path(self, path):
        return '/'.join([i for i in path.split('/') if i != ''])    
   
    def list_blobs(self, path, file_name_pattern=None):
        """
        Returns a list of blobs in the specified path. The path should be to a folder, not a file.
        The file_name_pattern parameter can be used to filter the results using a wildcard pattern for the file name.
        Example: file_name_pattern='*1980*.csv' will return all blobs that contain '1980' in the file name.
        """
        full_path = self.__get_full_path(path)        
        blobs = self.blob_container_client.list_blobs(name_starts_with=full_path)

        if file_name_pattern is None:
            return blobs

        # Filter the blobs by the file name pattern and exclude any folders
        filtered_blobs = [blob for blob in blobs if fnmatch.fnmatch(blob.name, file_name_pattern)
                          and blob.content_settings.content_type is not None]

        return filtered_blobs

    def file_exists(self, path) -> bool:
        full_path = self.__get_full_path(path)             
        blob = self.blob_container_client.get_blob_client(full_path)        
        return blob.exists()
        

    def read_file_content(self, path, read_into_stream=True) -> bytes:
        full_path = self.__get_full_path(path)        
        if read_into_stream:
            blob = self.blob_container_client.get_blob_client(full_path)
            stream = BytesIO()
            blob.download_blob().readinto(stream)
            return stream.getvalue()
        else:
            return self.blob_container_client.download_blob(full_path).content_as_bytes()
       

    def write_file_content(self, path, content, overwrite=True, lease=None):
        full_path = self.__get_full_path(path)          
        blob = self.blob_container_client.get_blob_client(full_path)
        blob.upload_blob(content, overwrite=overwrite, lease=lease)
       
    
    def delete_file(self, path):
        full_path = self.__get_full_path(path)       
        self.blob_container_client.delete_blob(full_path, delete_snapshots='include')