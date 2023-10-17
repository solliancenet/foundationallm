from foundationallm.hubs import Metadata

class SearchServiceAuthenticationMetadata(Metadata):
    """Represents search service connection and authentication details"""
    endpoint: str
    key_secret: str