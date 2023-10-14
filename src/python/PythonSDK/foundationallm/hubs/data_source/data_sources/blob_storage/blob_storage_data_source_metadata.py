from typing import List, Optional
from .blob_storage_file_type import BlobStorageFileType
from .blob_storage_authentication_metadata import BlobStorageAuthenticationMetadata
from foundationallm.hubs.data_source import DataSourceMetadata


class BlobStorageDataSourceMetadata(DataSourceMetadata):
    file_type: BlobStorageFileType
    authentication: BlobStorageAuthenticationMetadata
    container: str
    files: Optional[List[str]] = None
    
