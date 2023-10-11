from pydantic import BaseModel
import typing

from foundationallm.langchain.data_sources.sql import SqlDbConfig

from foundationallm.langchain.data_sources import DataSourceConfiguration

class DataSourceMetadata(BaseModel):
    name: str
    type: str
    description: str
    configuration: SqlDbConfig #DataSourceConfiguration