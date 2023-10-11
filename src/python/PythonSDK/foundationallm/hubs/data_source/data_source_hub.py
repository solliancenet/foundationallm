from foundationallm.credentials import AzureCredential
from foundationallm.config import Configuration
from foundationallm.hubs import HubBase
from foundationallm.hubs.data_source import DataSourceRepository, DataSourceResolver

class DataSourceHub(HubBase):
    """The DataSourceHub is responsible for resolving data sources."""

    def __init__(self):
        # initialize config       
        key_vault_name = Configuration().get_value(key="foundationallm-keyvault-name")        
        self.config = Configuration(keyvault_name=key_vault_name, credential=AzureCredential())
        super().__init__(repository= DataSourceRepository(self.config), resolver= DataSourceResolver())
        