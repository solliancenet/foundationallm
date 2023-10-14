from foundationallm.hubs import Metadata
from typing import Optional

class BlobStorageAuthenticationMetadata(Metadata):
    connection_string_secret: str