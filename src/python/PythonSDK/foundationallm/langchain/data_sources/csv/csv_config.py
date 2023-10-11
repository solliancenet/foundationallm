from foundationallm.langchain.data_sources import DataSourceConfiguration

class CSVConfig(DataSourceConfiguration):
    source_file_path: str
    path_value_is_secret: bool