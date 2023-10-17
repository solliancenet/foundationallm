from foundationallm.hubs import Repository
from foundationallm.hubs.data_source import DataSourceMetadata, UnderlyingImplementation
from .data_sources.sql import SQLDataSourceMetadata
from .data_sources.blob_storage import BlobStorageDataSourceMetadata
from .data_sources.search_service import SearchServiceDataSourceMetadata
from foundationallm.hubs.data_source import DataSourceHubStorageManager
from typing import List

class DataSourceRepository(Repository):
    """ The DataSourceRepository is responsible for retrieving data source metadata from storage."""
    
    def get_metadata_values(self, pattern:List[str]=None) -> List[DataSourceMetadata]:
        """
        Returns a list of DataSourceMetadata objects, optionally filtered by a pattern.
        D:\SourceCode\Solliance\foundationallm\src\python\PythonSDK\foundationallm\hubs\prompt
        Background: Agents may have allowed datasources defined. In storage, they are stored as JSON files with the naming pattern
        of datasourcename.json.
        
        Args:
        pattern (List[str]): The pattern defines the specific data sources to return (by name), if empty or None, return all data sources.
        
        """
        mgr = DataSourceHubStorageManager(config=self.config)
        config_files = []
        if pattern is None or len(pattern)== 0:
            config_files = mgr.list_blobs()
        else:
            config_files = [datasourcename +".json" for datasourcename in pattern]
       
        configs = []
        for config_file in config_files:
            common_datasource_metadata = {}
            try:
                common_datasource_metadata = DataSourceMetadata.model_validate_json(mgr.read_file_content(config_file))                
                if common_datasource_metadata.underlying_implementation == UnderlyingImplementation.SQL:                
                    configs.append(SQLDataSourceMetadata.model_validate_json(mgr.read_file_content(config_file)))
                elif common_datasource_metadata.underlying_implementation == UnderlyingImplementation.BLOB_STORAGE:
                    configs.append(BlobStorageDataSourceMetadata.model_validate_json(mgr.read_file_content(config_file)))
                elif common_datasource_metadata.underlying_implementation == UnderlyingImplementation.SEARCH_SERVICE:
                    configs.append(SearchServiceDataSourceMetadata.model_validate_json(mgr.read_file_content(config_file)))
            except:
                continue
        return configs
    
    def get_metadata_by_name(self, name: str) -> DataSourceMetadata:
        """
        Returns a DataSourceMetadata object by its name.
        Args:
        name:str is the name of the data source as well as the name of the configuration file.
        """
        mgr = DataSourceHubStorageManager(config=self.config)
        config_file = name + ".json"
        common_datasource_metadata = DataSourceMetadata.model_validate_json(mgr.read_file_content(config_file))
        config = None
        if common_datasource_metadata.underlying_implementation == UnderlyingImplementation.SQL:
            config = SQLDataSourceMetadata.model_validate_json(mgr.read_file_content(config_file))
        elif common_datasource_metadata.underlying_implementation == UnderlyingImplementation.BLOB_STORAGE:
            config = BlobStorageDataSourceMetadata.model_validate_json(mgr.read_file_content(config_file))       
        return config
      
