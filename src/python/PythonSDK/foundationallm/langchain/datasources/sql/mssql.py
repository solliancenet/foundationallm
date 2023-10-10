from langchain.sql_database import SQLDatabase

from foundationallm.langchain.datasources.sql import SqlDbConfig

class MsSqlServer():
    """
    Represents a connection to a Microsoft SQL Server database.
    """
    
    def __init__(self, config: SqlDbConfig):
        self.config = config

    def get_database(self) -> SQLDatabase:
        return SQLDatabase.from_uri(
            self.config.get_connection_string(),
            include_tables=self.config.include_tables,
            sample_rows_in_table_info=2 # This should also be in the config object
        )