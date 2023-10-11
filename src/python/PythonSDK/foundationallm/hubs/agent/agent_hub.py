from foundationallm.credentials import AzureCredential
from foundationallm.config import Configuration
from foundationallm.hubs.agent import AgentRepository, AgentResolver
from foundationallm.hubs import HubBase

class AgentHub(HubBase):
    """The AgentHub is responsible for resolving agents."""
    def __init__(self):        
        # initialize config       
        key_vault_name = Configuration().get_value(key="foundationallm-keyvault-name")        
        self.config = Configuration(keyvault_name=key_vault_name, credential=AzureCredential())
        super().__init__(repository=AgentRepository(self.config), resolver=AgentResolver())
     