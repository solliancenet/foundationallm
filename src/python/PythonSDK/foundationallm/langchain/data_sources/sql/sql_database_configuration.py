from typing import List, Optional
from foundationallm.langchain.data_sources import DataSourceConfiguration

class SQLDatabaseConfiguration(DataSourceConfiguration):
    """Configuration settings for a connection to a SQL database."""
    dialect: str
    host: str
    port: int = None
    database_name: str
    username: str = None
    password_secret_setting_key_name: str = None
    include_tables: Optional[List[str]] = None
    exclude_tables: Optional[List[str]] = None
    few_shot_example_count: int = 0
    row_level_security_enabled: bool = False
    schema: str = None
