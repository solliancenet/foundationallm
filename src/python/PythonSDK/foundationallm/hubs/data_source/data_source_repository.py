from foundationallm.hubs import Repository
from foundationallm.hubs.data_source import DataSourceMetadata
from foundationallm.hubs.data_source import UnderlyingImplementation
from foundationallm.data_sources.blob_storage import BlobStorageDataSourceMetadata
from foundationallm.hubs.data_source import DataSourceHubStorageManager
import foundationallm.data_sources.sql.sql_data_source_metadata as SQLDataSource

from typing import List

class DataSourceRepository(Repository):
    """ The DataSourceRepository is responsible for retrieving data source metadata from storage."""
    
    def get_metadata_values(self) -> List[DataSourceMetadata]:        
        mgr = DataSourceHubStorageManager(config=self.config)
        config_files = mgr.list_blobs()
        configs = []
        for config_file in config_files:
            commonDatasourceMetadata = DataSourceMetadata.model_validate_json(mgr.read_file_content(config_file))            
            if commonDatasourceMetadata.underlying_implementation == UnderlyingImplementation.SQL:
                configs.append(SQLDataSource.model_validate_json(mgr.read_file_content(config_file)))
            elif commonDatasourceMetadata.underlying_implementation == UnderlyingImplementation.BLOB_STORAGE:
                configs.append(BlobStorageDataSourceMetadata.model_validate_json(mgr.read_file_content(config_file)))        
        return configs
