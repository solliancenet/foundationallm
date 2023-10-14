from typing import Union

from .metadata_base import MetadataBase
from foundationallm.langchain.data_sources.csv import CSVConfiguration
from foundationallm.langchain.data_sources.sql import SQLDatabaseConfiguration

class DataSource(MetadataBase):
    """Data source metadata model."""
    configuration: Union[CSVConfiguration, SQLDatabaseConfiguration]