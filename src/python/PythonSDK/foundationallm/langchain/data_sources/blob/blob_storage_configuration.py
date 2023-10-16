from typing import List, Optional
from foundationallm.langchain.data_sources import DataSourceConfiguration

class BlobStorageConfiguration(DataSourceConfiguration):
    connection_string_secret: str
    container: str
    files: Optional[List[str]] = None