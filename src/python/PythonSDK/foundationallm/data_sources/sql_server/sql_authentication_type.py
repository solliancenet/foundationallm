from enum import Enum, auto


class SQLAuthenticationType(Enum):
    CONNECTION_STRING = auto()
    USERNAME_PASSWORD = auto()
