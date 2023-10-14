from foundationallm.hubs import Metadata
from .sql_authentication_type import SQLAuthenticationType
from typing import Optional

class SQLAuthenticationMetadata(Metadata):
    authentication_type: SQLAuthenticationType
    connection_string_secret: Optional[str] = None
    host: Optional[str] = None
    port: Optional[int] = None
    database: Optional[str] = None
    username: Optional[str] = None
    password_secret: Optional[str] = None
