from abc import abstractmethod

from langchain_community.utilities.sql_database import SQLDatabase

from foundationallm.config import Configuration
from foundationallm.langchain.data_sources.sql import SQLDatabaseConfiguration

class SQLDatabaseDataSource:
    """Data source object for a SQL database."""
    dialect: str
    driver: str
    host: str
    port: int = None
    database_name: str
    username: str
    password: str

    def __init__(self, sql_db_config: SQLDatabaseConfiguration, config: Configuration):
        """
        Initializes a SQL database data source.

        Parameters
        ----------
        sql_db_config : SqlDbConfig
            SQL Database configuration class for providing settings to the database.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.sql_db_config = sql_db_config
        self.dialect = sql_db_config.dialect
        self.driver = self.get_driver()
        self.host = sql_db_config.host
        self.port = sql_db_config.port or self.get_default_port()
        self.database_name = sql_db_config.database_name
        self.username = sql_db_config.username
        self.password = config.get_value(self.sql_db_config.password_secret_setting_key_name)
        self.include_tables = sql_db_config.include_tables
        self.sample_rows_in_table_info = 0 if sql_db_config.row_level_security_enabled \
                                            else sql_db_config.few_shot_example_count
        self.row_level_security_enabled = sql_db_config.row_level_security_enabled

    def get_database(self) -> SQLDatabase:
        """
        Retrieves a SQLDatabase object
        
        Returns
        -------
        SQLDatabase
            Returns a connection to a SQL database.
        """
        return SQLDatabase.from_uri(
            database_uri = self.get_connection_string(),
            include_tables=self.include_tables,
            sample_rows_in_table_info=self.sample_rows_in_table_info
        )

    @abstractmethod
    def get_connection_string(self) -> str:
        """retrieves the correct connection string for a given database type"""

    @abstractmethod
    def get_driver(self) -> str:
        """Retrieves the driver to use for connecting to the database."""

    @abstractmethod
    def get_default_port(self) -> int:
        """Retrieves the default port for the database."""
