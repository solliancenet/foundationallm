from urllib import parse

from foundationallm.langchain.data_sources.sql import SQLDatabaseDataSource

class MariaDB(SQLDatabaseDataSource):
    """
    Generates a MariaDB database connection.
    """
    def get_connection_string(self) -> str:
        """
        Gets a formatted connection string for connecting to the database.
        
        Returns
        -------
        int
            Returns a formatted connection string for connecting to the database.
        """
        return f'{self.dialect}+{self.driver}://{self.username}:{parse.quote_plus(self.password)}@{self.host}/{self.database_name}?charset=utf8mb4'
    
    def get_driver(self) -> str:
        """
        Gets the driver for connecting to the database.
        
        Returns
        -------
        str
            Returns the driver for connecting to the database.
        """
        return 'pymysql'
    
    def get_default_port(self) -> int:
        """
        Gets the default port of the database.
        
        Returns
        -------
        int
            Returns the default port for the database.
        """
        return 3306