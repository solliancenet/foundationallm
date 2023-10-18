from typing import List, Optional
from .blob_storage_authentication_metadata import BlobStorageAuthenticationMetadata
from foundationallm.hubs.data_source import DataSourceMetadata


class BlobStorageDataSourceMetadata(DataSourceMetadata):    
    authentication: BlobStorageAuthenticationMetadata
    container: str
    files: Optional[List[str]] = None
    
