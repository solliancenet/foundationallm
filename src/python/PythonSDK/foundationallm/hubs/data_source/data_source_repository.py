from foundationallm.hubs import Repository
from foundationallm.hubs.data_source import DataSourceMetadata, DataSourceHubStorageManager, UnderlyingImplementation
from foundationallm.data_sources.sql import SQLDataSourceMetadata
from foundationallm.data_sources.blob_storage import BlobStorageDataSourceMetadata
from foundationallm.data_sources.search_service import SearchServiceDataSourceMetadata
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
                configs.append(SQLDataSourceMetadata.model_validate_json(mgr.read_file_content(config_file)))
            elif commonDatasourceMetadata.underlying_implementation == UnderlyingImplementation.BLOB_STORAGE:
                configs.append(BlobStorageDataSourceMetadata.model_validate_json(mgr.read_file_content(config_file)))        
        return configs
    
    def get_metadata_by_name(self, name: str) -> DataSourceMetadata:
        # The name parameter is the name of the data source as well as the name of the configuration file.
        mgr = DataSourceHubStorageManager(config=self.config)
        config_file = name + ".json"
        commonDatasourceMetadata = DataSourceMetadata.model_validate_json(mgr.read_file_content(config_file))
        config = None
        if commonDatasourceMetadata.underlying_implementation == UnderlyingImplementation.SQL_SERVER:
            config = SQLServerDataSourceMetadata.model_validate_json(mgr.read_file_content(config_file))
        elif commonDatasourceMetadata.underlying_implementation == UnderlyingImplementation.BLOB_STORAGE:
            config = BlobStorageDataSourceMetadata.model_validate_json(mgr.read_file_content(config_file))
        elif commonDatasourceMetadata.underlying_implementation == UnderlyingImplementation.SEARCH_SERVICE:
            config = SearchServiceDataSourceMetadata.model_validate_json(mgr.read_file_content(config_file))
        return config
      
