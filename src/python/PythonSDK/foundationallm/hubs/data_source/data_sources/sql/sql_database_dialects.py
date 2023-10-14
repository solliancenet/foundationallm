from enum import Enum

class SQLDatabaseDialect(str, Enum):
    """Enumerator of SQL database dialects"""
    MARIADB = "mariadb"
    MSSQL = "mssql"
    MYSQL = "mysql"
    POSTGRESQL = "postgresql"