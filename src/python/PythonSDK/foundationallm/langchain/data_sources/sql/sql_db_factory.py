from langchain.sql_database import SQLDatabase

#from foundationallm.credentials import AzureCredential
from foundationallm.config import Configuration
from foundationallm.langchain.data_sources.sql import SqlDbConfig
#from foundationallm.langchain.data_sources.sql import SqlDbDataSource
from foundationallm.langchain.data_sources.sql.mssql import MsSqlServer

class SqlDbFactory():
    """
    Factory class to determine which SQL database to use.
    """

    def __init__(self, sql_db_config: SqlDbConfig, app_config: Configuration):
        self.sql_db_config: SqlDbConfig = sql_db_config
        self.app_config: Configuration = app_config
        self.dialect = self.sql_db_config.dialect
        
    def get_sql_database(self) -> SQLDatabase:
        match self.dialect:
            case 'mssql':
                return MsSqlServer(sql_db_config = self.sql_db_config, app_config = self.app_config).get_database()
        