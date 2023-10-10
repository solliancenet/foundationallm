from typing import List, Optional
from foundationallm.data_sources.blob_storage import BlobStorageFileType, BlobStorageAuthenticationMetadata
from foundationallm.hubs.data_source import DataSourceMetadata


class BlobStorageDataSourceMetadata(DataSourceMetadata):
    file_type: BlobStorageFileType
    authentication: BlobStorageAuthenticationMetadata
    container: str
    files: Optional[List[str]] = None
    
