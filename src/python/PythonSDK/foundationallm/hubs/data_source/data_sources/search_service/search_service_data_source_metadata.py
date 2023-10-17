from foundationallm.hubs import Metadata
from foundationallm.hubs.data_source import DataSourceMetadata
from .search_service_authentication_metadata import SearchServiceAuthenticationMetadata

class SearchServiceDataSourceMetadata(DataSourceMetadata):
    """Represents search service data source that details authentication, connnection, and details on the content contained within"""
    index_name: str
    authentication: SearchServiceAuthenticationMetadata