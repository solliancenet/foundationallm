from foundationallm.storage import StorageManagerBase
from azure.storage.blob import BlobServiceClient, ContainerClient
from io import BytesIO
from zipfile import ZIP_DEFLATED
import fnmatch


class BlobStorageManager(StorageManagerBase):
    blob_connection_string: str = None
    container_name: str = None
    root_path: str = None
    blob_service_client: BlobServiceClient = None
    blob_container_client: ContainerClient = None

    def __init__(self, blob_connection_string=None, container_name=None,
                 root_path=None):

        if blob_connection_string is None or blob_connection_string == '':
            raise ValueError(f'The blob_connection_string parameter must be set to a valid connection string.')
        if container_name is None or container_name == '':
            raise ValueError(f'The container_name parameter must be set to a valid container.')
      
        self.blob_connection_string = blob_connection_string
        self.container_name = container_name
        self.root_path = root_path
        self.blob_service_client = BlobServiceClient.from_connection_string(self.blob_connection_string)
        self.blob_container_client = self.blob_service_client.get_container_client(container_name)
        

    def __get_full_path(self, path, root_path=None):
        root = (self.root_path) \
            if root_path is None else root_path
        return '/'.join([i for i in [root] + path.split('/') if i != ''])
    
    def __get_container(self, container_name):
        if container_name is None:
            return self.blob_container_client
        return self.blob_service_client.get_container_client(container_name)
    
    def list_blobs(self, path, containerName=None, root_path=None, file_name_pattern=None):
        """
        Returns a list of blobs in the specified path. The path should be to a folder, not a file.
        The file_name_pattern parameter can be used to filter the results using a wildcard pattern for the file name.
        Example: file_name_pattern='*1980*.csv' will return all blobs that contain '1980' in the file name.
        """
        full_path = self.__get_full_path(path, root_path)
        container = self.__get_container(containerName)

        blobs = container.list_blobs(name_starts_with=full_path)

        if file_name_pattern is None:
            return blobs

        # Filter the blobs by the file name pattern and exclude any folders
        filtered_blobs = [blob for blob in blobs if fnmatch.fnmatch(blob.name, file_name_pattern)
                          and blob.content_settings.content_type is not None]

        return filtered_blobs

    def file_exists(self, path, container_name=None, root_path=None) -> bool:
        full_path = self.__get_full_path(path, root_path)
        container = self.__get_container(container_name)       
        blob = container.get_blob_client(full_path)        
        return blob.exists()
        

    def read_file_content(self, path, containerName=None, root_path=None, read_into_stream=True) -> bytes:
        full_path = self.__get_full_path(path, root_path)
        container = self.__get_container(containerName)

        if read_into_stream:
            blob = container.get_blob_client(full_path)
            stream = BytesIO()
            blob.download_blob().readinto(stream)
            return stream.getvalue()
        else:
            return container.download_blob(full_path).content_as_bytes()
       

    def write_file_content(self, path, content, overwrite=True, containerName=None,
                           root_path=None, lease=None):
        full_path = self.__get_full_path(path, root_path)
        container = self.__get_container(containerName)       
        blob = container.get_blob_client(full_path)
        blob.upload_blob(content, overwrite=overwrite, lease=lease)
       
    
    def delete_file(self, path, containerName=None, root_path=None):
        full_path = self.__get_full_path(path, root_path)
        container = self.__get_container(containerName)
        container.delete_blob(full_path, delete_snapshots='include')