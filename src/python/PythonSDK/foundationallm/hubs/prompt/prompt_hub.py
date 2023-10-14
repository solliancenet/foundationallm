from foundationallm.credentials import AzureCredential
from foundationallm.config import Configuration
from foundationallm.hubs.prompt import PromptResolver, PromptRepository
from foundationallm.hubs import HubBase

class PromptHub(HubBase):
    """The PromptHub class is responsible for resolving prompts."""
    
    def __init__(self):
         # initialize config            
        key_vault_name = Configuration().get_value(key="foundationallm-keyvault-name")        
        self.config = Configuration(keyvault_name=key_vault_name, credential=AzureCredential())
        super().__init__( resolver=PromptResolver(PromptRepository(self.config)))