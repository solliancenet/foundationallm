import json
from urllib import parse

class SqlDbConfig:
    def __init__(self, dialect: str, host: str, database: str, username: str, password: str, prompt_prefix: str, include_tables: list[str] = [], exclude_tables: list[str] = []): # TODO: Allow port to be passed in and get default if empty.
        self.dialect = dialect
        self.driver= self._get_db_driver()
        self.host = host
        self.port = self._get_db_default_port()
        self.database = database
        self.username = username
        self.password = password
        self.include_tables = include_tables or []
        self.exclude_tables = exclude_tables or []
        self.prompt_prefix = prompt_prefix

    def __str__(self):
        return json.dumps(self.__dict__)
    
    def get_connection_string(self) -> str:
        if self.dialect == 'mssql':
            return f'{self.dialect}+{self.driver}://{self.username}:{parse.quote_plus(self.password)}@{self.host}:{self.port}/{self.database}?driver={parse.quote_plus("ODBC Driver 18 for SQL Server")}'
    
    def __get_db_driver(self):
        match self.dialect:
            case 'mssql':
                return 'pyodbc'
            case 'postgresql':
                return 'pyscopg2'
        
    def __get_db_default_port(self):
        match self.dialect:
            case 'mssql':
                return 1433
            case 'postgresql':
                return 5432