from foundationallm.hubs import Metadata
from typing import Optional

class SearchServiceAuthenticationMetadata(Metadata):
    endpoint: str
    key_secret: str