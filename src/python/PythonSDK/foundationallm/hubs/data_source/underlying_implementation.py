from enum import Enum, auto

class UnderlyingImplementation(Enum):
    """The UnderlyingImplementation enum is used to indicate the underlying implementation of a data source."""
    SQL = "sql"
    BLOB_STORAGE = "blob-storage"
    SEARCH_SERVICE = "search-service"
    
