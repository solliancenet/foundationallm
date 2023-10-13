from enum import Enum, auto


class UnderlyingImplementation(Enum):
    """The UnderlyingImplementation enum is used to indicate the underlying implementation of a data source."""
    SQL = auto()
    BLOB_STORAGE = auto()
    SEARCH_SERVICE = auto()
