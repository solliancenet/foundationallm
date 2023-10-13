from foundationallm.langchain.data_sources import DataSourceConfiguration

class CSVConfiguration(DataSourceConfiguration):
    """Configuration settings for a connection to a CSV file."""
    source_file_path: str
    path_value_is_secret: bool