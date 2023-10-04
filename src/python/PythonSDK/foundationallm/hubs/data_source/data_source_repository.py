from foundationallm.hubs import Repository
from foundationallm.hubs.data_source import DataSourceMetadata, UnderlyingImplementation
from foundationallm.datasources import SQLServerDataSourceMetadata, SQLAuthenticationMetadata, SQLAuthenticationType
from typing import List


class DataSourceRepository(Repository):
    def get_metadata_values(self) -> List[DataSourceMetadata]:        
        sql_config = SQLServerDataSourceMetadata(name="cocorahs", description="Weather database", few_shot_examples=None,
                                              underlying_implementation=UnderlyingImplementation.SQL_SERVER,
                                              authentication=SQLAuthenticationMetadata(authentication_type=SQLAuthenticationType.USERNAME_PASSWORD,
                                                                                       host="cocorahs-ai.database.windows.net",
                                                                                       port=1433,
                                                                                       database="cocorahs",
                                                                                       username="cocorahs",
                                                                                       password_secret="cocorahs-db-password"),
                                              tables=["DailyPrecipReport", "HailReport", "Observer", "ObserverStatus", "ObserverType", "Station"])
                                                                                       
        return [sql_config]
