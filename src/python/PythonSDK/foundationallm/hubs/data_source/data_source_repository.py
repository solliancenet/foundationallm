from foundationallm.hubs import Repository
from foundationallm.hubs.data_source import DataSourceMetadata, UnderlyingImplementation
from foundationallm.datasources.sql_server import SQLServerDataSourceMetadata, SQLAuthenticationMetadata, SQLAuthenticationType
from typing import List


class DataSourceRepository(Repository):
    def get_metadata_values(self) -> List[DataSourceMetadata]:        
        sql_config = SQLServerDataSourceMetadata(name="weatherdb", description="Weather database", few_shot_examples=None,
                                              underlying_implementation=UnderlyingImplementation.SQL_SERVER,
                                              authentication=SQLAuthenticationMetadata(authentication_type=SQLAuthenticationType.USERNAME_PASSWORD,
                                                                                       host="weatherdb.database.windows.net",
                                                                                       port=1433,
                                                                                       database="weatherdb",
                                                                                       username="weatherdbreader",
                                                                                       password_secret="weather-db-password"),
                                              tables=["Rain", "Hail", "Observer", "ObserverType", "Station"])
                                                                                       
        return [sql_config]
