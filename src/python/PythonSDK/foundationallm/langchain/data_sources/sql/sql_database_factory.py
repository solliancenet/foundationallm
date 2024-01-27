from langchain_community.utilities.sql_database import SQLDatabase

from foundationallm.config import Configuration
from foundationallm.langchain.data_sources.sql import SQLDatabaseConfiguration
from foundationallm.langchain.data_sources.sql import MariaDB, MicrosoftSQLServer, MySQL, PostgreSQL
from foundationallm.hubs.data_source.data_sources.sql import SQLDatabaseDialect

class SQLDatabaseFactory():
    """
    Factory class to determine which SQL database to use.
    """
    def __init__(self, sql_db_config: SQLDatabaseConfiguration, config: Configuration):
        """
        Initializes an AgentFactory for selecting which agent to use for completion.

        Parameters
        ----------
        sql_db_config : SQLDatabaseConfiguration
            SQL Database configuration class for providing settings to the database.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.sql_db_config: SQLDatabaseConfiguration = sql_db_config
        self.config: Configuration = config
        self.dialect = self.sql_db_config.dialect

    def get_sql_database(self) -> SQLDatabase:
        match self.dialect:
            case SQLDatabaseDialect.MARIADB:
                return MariaDB(sql_db_config = self.sql_db_config,
                               config = self.config).get_database()
            case SQLDatabaseDialect.MSSQL:
                return MicrosoftSQLServer(sql_db_config = self.sql_db_config,
                                          config = self.config).get_database()
            case SQLDatabaseDialect.MYSQL:
                return MySQL(sql_db_config = self.sql_db_config,
                             config = self.config).get_database()
            case SQLDatabaseDialect.POSTGRESQL:
                return PostgreSQL(sql_db_config = self.sql_db_config,
                                  config = self.config).get_database()
