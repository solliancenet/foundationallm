from pydantic import BaseModel
from typing import List, Union
from .data_source_metadata import DataSourceMetadata
from foundationallm.data_sources.sql import SQLDataSourceMetadata
from foundationallm.data_sources.blob_storage import BlobStorageDataSourceMetadata

class DataSourceHubResponse(BaseModel):
    data_sources: List[Union[DataSourceMetadata, SQLDataSourceMetadata, BlobStorageDataSourceMetadata]]