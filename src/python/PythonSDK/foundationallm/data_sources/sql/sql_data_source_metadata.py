from typing import List, Optional
from foundationallm.data_sources.sql import SQLAuthenticationMetadata, SQLDialect
from foundationallm.hubs.data_source import DataSourceMetadata

class SQLDataSourceMetadata(DataSourceMetadata):
    authentication: Optional[SQLAuthenticationMetadata] = None
    dialect: SQLDialect
    include_tables: Optional[List[str]] = None
    exclude_tables: Optional[List[str]] = None
    few_shot_example_count: Optional[int] = None
