from foundationallm.hubs import Repository
from foundationallm.hubs.data_source import DataSourceMetadata, DataSourceHubStorageManager, UnderlyingImplementation
from foundationallm.data_sources.sql_server import SQLServerDataSourceMetadata
from typing import List

class DataSourceRepository(Repository):
    """ The DataSourceRepository is responsible for retrieving data source metadata from storage."""
    
    def get_metadata_values(self) -> List[DataSourceMetadata]:        
        mgr = DataSourceHubStorageManager(config=self.config)
        config_files = mgr.list_blobs()
        configs = []
        for config_file in config_files:
            commonDatasourceMetadata = DataSourceMetadata.model_validate_json(mgr.read_file_content(config_file))
            if commonDatasourceMetadata.underlying_implementation == UnderlyingImplementation.SQL_SERVER:
                configs.append(SQLServerDataSourceMetadata.model_validate_json(mgr.read_file_content(config_file)))        
        return configs
