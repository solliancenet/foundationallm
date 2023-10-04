from foundationallm.hubs import BaseHub
from foundationallm.hubs.data_source import DataSourceRepository, DataSourceResolver

class DataSourceHub(BaseHub):
    def __init__(self):
        self.repository = DataSourceRepository()
        self.resolver = DataSourceResolver()