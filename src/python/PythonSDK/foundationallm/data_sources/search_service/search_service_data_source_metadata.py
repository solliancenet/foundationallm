from typing import List, Optional
from foundationallm.data_sources.search_service import SearchServiceAuthenticationMetadata
from foundationallm.hubs.data_source import DataSourceMetadata


class SearchServiceDataSourceMetadata(DataSourceMetadata):    
    authentication: SearchServiceAuthenticationMetadata
    index_name: str
    
