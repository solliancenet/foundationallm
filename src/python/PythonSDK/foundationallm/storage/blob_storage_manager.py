from io import BytesIO
import base64
import fnmatch
from azure.storage.blob import BlobServiceClient
from foundationallm.storage import StorageManagerBase
from azure.identity import DefaultAzureCredential

class BlobStorageManager(StorageManagerBase):
    """
    The BlobStorageManager class is responsible for managing files in Azure Blob Storage.
    """

    def __init__(self, blob_connection_string=None, container_name=None, account_name=None, authentication_type='ConnectionString'):
        """
        Initialize a blob storage manager.

        Parameters
        ----------
        blob_connection_string : str
            The connection string for the target blob storage account.
        container_name : str
            The name of the container is blob storage from which blobs should be retrieved.
        """
        if authentication_type == 'AzureIdentity':
            if account_name is None or account_name == '':
                raise ValueError('The account_name parameter must be set to a valid account name.')
            credential = DefaultAzureCredential(exclude_environment_credential=True)
            blob_service_client = BlobServiceClient(account_url=f"https://{account_name}.blob.core.windows.net", credential=credential)
        else:
            if blob_connection_string is None or blob_connection_string == '':
                raise ValueError('The blob_connection_string parameter must be set to a valid connection string.')
            blob_service_client = BlobServiceClient.from_connection_string(blob_connection_string)
            
        if container_name is None or container_name == '':
            raise ValueError('The container_name parameter must be set to a valid container.')
 
        self.blob_container_client = blob_service_client.get_container_client(container_name)

    def __get_full_path(self, path) -> str:
        """
        Retrieves the full path to the target blob.

        Parameters
        ----------
        path : str
            The path to the blob.

        Returns
        -------
        str
            The full path to the specified blob.
        """
        return '/'.join([i for i in path.split('/') if i != ''])

    def list_blobs(self, path, file_name_pattern=None):
        """
        Retrieves a list of blobs in the specified path. The path should be to a folder, not a file.

        Parameters
        ----------
        file_name_pattern : str
            Filter used to determine the results using a wildcard pattern for the file name.
            Example:
                file_name_pattern='*1980*.csv' returns all blobs that contain '1980' in the file name.

        Returns
        -------
        List[BlobProperties]
            A list of blobs in the specified path.
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
        """
        Checks whether a specified path exists in blob storage.

        Parameters
        ----------
        path : str
            The path the check for existence.

        Returns
        -------
        bool
            Returns true of the specified path exists. Otherwise, false.
        """
        full_path = self.__get_full_path(path)
        blob = self.blob_container_client.get_blob_client(full_path)
        return blob.exists()

    def read_file_content(self, path, read_into_stream=True) -> bytes:
        """
        Retrieves the contents of a specified file in bytes.

        Parameters
        ----------
        path : str
            The path to the blob being retrieved.
        read_into_stream : boolean
            Flag indicating whether to read the file content into a byte stream. Default is True.

        Returns
        -------
        bytes
            Returns the bytes, or a stream of bytes, representing the content of the specified file
            or None if the file does not exist.
        """
        if self.file_exists(path):
            full_path = self.__get_full_path(path)
            if read_into_stream:
                blob = self.blob_container_client.get_blob_client(full_path)
                stream = BytesIO()
                blob.download_blob().readinto(stream)
                return stream.getvalue()
            else:
                blob = self.blob_container_client.download_blob(full_path)
                return blob.content_as_bytes()
        else:
            return None

    def read_file_content_as_base64(self, path) -> str:
        """
        Retrieves the contents of a specified file as a base64 string.
        Parameters
        ----------
        path : str
            The path to the blob being retrieved.
        Returns
        -------
        str
            Returns the content of the specified file as a base64 string.
        """       
        content = self.read_file_content(path)
        if content is None:
            return None
        return base64.b64encode(content).decode('utf-8')
    
    def write_file_content(self, path, content, overwrite=True, lease=None):
        """
        Writes data to a specified file.

        Parameters
        ----------
        path : str
            The path to the blob to which data is being written.
        content
            The data being written into the blob.
        overwrite : boolean
            Indicates whether the content of the blob should be overwritten by the incoming content.
            Default is True.
        lease
            The lease to use for accessing the blob. Default is None.
        """
        full_path = self.__get_full_path(path)
        blob = self.blob_container_client.get_blob_client(full_path)
        blob.upload_blob(content, overwrite=overwrite, lease=lease)

    def delete_file(self, path):
        """
        Deletes the specified blob.

        Parameters
        ----------
        path : str
            The path to the blob to be deleted.
        """
        full_path = self.__get_full_path(path)
        self.blob_container_client.delete_blob(full_path, delete_snapshots='include')
