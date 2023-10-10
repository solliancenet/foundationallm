from typing import List, Optional
from foundationallm.data_sources.sql_server import SQLAuthenticationMetadata
from foundationallm.hubs.data_source import DataSourceMetadata


class SQLServerDataSourceMetadata(DataSourceMetadata):
    authentication: Optional[SQLAuthenticationMetadata] = None
    tables: Optional[List[str]] = None
