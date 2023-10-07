from foundationallm.hubs import Repository
from foundationallm.hubs.data_source import DataSourceMetadata, DataSourceStorageManager, UnderlyingImplementation
from foundationallm.datasources.sql_server import SQLServerDataSourceMetadata
from typing import List

class DataSourceRepository(Repository):
    def get_metadata_values(self) -> List[DataSourceMetadata]:        
        mgr = DataSourceStorageManager()
        config_files = mgr.list_blobs()
        configs = []
        for config_file in config_files:
            commonDatasourceMetadata = DataSourceMetadata.model_validate_json(mgr.read_file_content(config_file))
            if commonDatasourceMetadata.underlying_implementation == UnderlyingImplementation.SQL_SERVER:
                configs.append(SQLServerDataSourceMetadata.model_validate_json(mgr.read_file_content(config_file)))        
        return configs
