from pydantic import BaseModel

from foundationallm.langchain.data_sources import DataSourceConfiguration

class DataSourceMetadata(BaseModel):
    name: str
    category: str
    description: str
    configuration: DataSourceConfiguration