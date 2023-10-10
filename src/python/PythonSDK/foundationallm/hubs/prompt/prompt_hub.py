from foundationallm.credentials import AzureCredential
from foundationallm.config import Configuration
from foundationallm.hubs.prompt import PromptRepository, PromptResolver
from foundationallm.hubs import HubBase

class PromptHub(HubBase):
    """The PromptHub class is responsible for resolving prompts."""
    
    def __init__(self):
         # initialize config
        credential = AzureCredential().get_credential()     
        key_vault_name = Configuration().get_value(key="foundationallm-keyvault-name")        
        self.config = Configuration(keyvault_name=key_vault_name, credential=credential)
        super().__init__(repository=PromptRepository(self.config), resolver=PromptResolver())