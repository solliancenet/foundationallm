from azure.identity import DefaultAzureCredential

class Credential:
    def get_credential(self):
        return DefaultAzureCredential()