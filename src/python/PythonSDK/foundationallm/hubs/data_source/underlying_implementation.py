from enum import Enum, auto


class UnderlyingImplementation(Enum):
    SQL_SERVER = auto()
    BLOB_STORAGE = auto()
