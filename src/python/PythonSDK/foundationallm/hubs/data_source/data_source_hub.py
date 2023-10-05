from foundationallm.hubs import HubBase
from foundationallm.hubs.data_source import DataSourceRepository, DataSourceResolver

class DataSourceHub(HubBase):
    def __init__(self):
        self.repository = DataSourceRepository()
        self.resolver = DataSourceResolver()