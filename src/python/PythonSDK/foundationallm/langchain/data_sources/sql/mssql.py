from urllib import parse

from foundationallm.langchain.data_sources.sql import SQLDatabaseDataSource

class MicrosoftSQLServer(SQLDatabaseDataSource):
    """
    Generates a Microsoft SQL Server database connection.
    """
    def get_connection_string(self) -> str:
        """
        Gets a formatted connection string for connecting to the database.
        
        Returns
        -------
        int
            Returns a formatted connection string for connecting to the database.
        """
        # TODO: Driver in querystring should come from config.
        return f'{self.dialect}+{self.driver}://{self.username}:{parse.quote_plus(self.password)}@{self.host}:{self.port}/{self.database_name}?driver={parse.quote_plus("ODBC Driver 18 for SQL Server")}&TrustServerCertificate=yes'

    def get_driver(self) -> str:
        """
        Gets the driver for connecting to the database.
        
        Returns
        -------
        str
            Returns the driver for connecting to the database.
        """
        return 'pyodbc'

    def get_default_port(self) -> int:
        """
        Gets the default port of the database.
        
        Returns
        -------
        int
            Returns the default port for the database.
        """
        return 1433
