from pydantic import BaseModel
from typing import Union

from foundationallm.langchain.data_sources.csv import CSVConfig
from foundationallm.langchain.data_sources.sql import SqlDbConfig
#from foundationallm.langchain.data_sources import DataSourceConfiguration

class DataSourceMetadata(BaseModel):
    name: str
    type: str
    description: str
    configuration: Union[CSVConfig, SqlDbConfig]