from enum import Enum, auto

class SQLAuthenticationType(Enum):
    CONNECTION_STRING = "connection-string"
    USERNAME_PASSWORD = "username-password"
