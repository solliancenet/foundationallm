from typing import List, Optional
from foundationallm.hubs.data_source import DataSourceMetadata
from foundationallm.data_sources.sql import SQLAuthenticationMetadata, SQLDatabaseDialect

class SQLDataSourceMetadata(DataSourceMetadata):
    authentication: Optional[SQLAuthenticationMetadata] = None
    dialect: SQLDatabaseDialect
    include_tables: Optional[List[str]] = None
    exclude_tables: Optional[List[str]] = None
    few_shot_example_count: Optional[int] = None
