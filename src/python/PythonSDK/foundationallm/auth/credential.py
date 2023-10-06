from azure.identity import DefaultAzureCredential


class Credential:
    def get_credential():
        return DefaultAzureCredential()
