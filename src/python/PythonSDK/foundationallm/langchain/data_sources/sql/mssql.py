from urllib import parse

from foundationallm.langchain.data_sources.sql import SqlDbDataSource

class MsSqlServer(SqlDbDataSource):
    """
    Generates a Microsoft SQL Server database connection.
    """
    def get_connection_string(self) -> str:
        # TODO: Driver in querystring should come from config.
        return f'{self.dialect}+{self.driver}://{self.username}:{parse.quote_plus(self.password)}@{self.host}:{self.port}/{self.database_name}?driver={parse.quote_plus("ODBC Driver 18 for SQL Server")}'
    
    def get_driver(self) -> str:
        return 'pyodbc'
    
    def get_default_port(self) -> int:
        return 1433