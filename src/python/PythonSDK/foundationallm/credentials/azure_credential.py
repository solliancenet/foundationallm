from foundationallm.credentials import Credential
from azure.identity import DefaultAzureCredential

class AzureCredential(Credential):
    def get_credential(self):
        return DefaultAzureCredential()