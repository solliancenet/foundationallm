import os
from azure.keyvault.secrets import SecretClient
from azure.identity import DefaultAzureCredential, ManagedIdentityCredential
from tenacity import retry, wait_random_exponential, stop_after_attempt, RetryError
import logging

class Configuration():
    keyvault_name: str = None
    
    def __init__(self, keyvault_name:str) :
        self.keyvault_name = keyvault_name
        

    def get_value(self, name: str, default: str = None) -> str:
        """
        Retrieves the value from a variable, and if not found atemtps to get the default 
        config value.

        If useKeyVault is set to True, atempts to retrieve the value from Azure Key Vault
        with the kv_name

        Parameters
        ----------
        - name : str 
            The name of the env variable to retrieve.
        - default : str
            Default value if variable not found.
        - useKeyVault : bool
            Determines wether to use Azure Key Vault
        - kv_name : str
            The name to retrieve if useKeyVault parameter is set to True

        Returns
        -------
        The value of the environment variable if it exists, or the Key Vault
        value for the variable.
        """

       
        try:    
            value = self.get_keyvault_value(name, self.keyvault_name)
            return value
            
        except Exception as e:
            pass

        value = os.environ.get(name)

        if value is not None:
            return value

        # If name not found as an env variable
        else:
            if default:
                return default
            else:
                return self.default_config_value(name)

    def __retry_before_sleep(retry_state):
        # Log the outcome of each retry attempt.
        message = f'Retrying {retry_state.fn}: attempt {retry_state.attempt_number} ended with: {retry_state.outcome}'
        if retry_state.outcome.failed:
            ex = retry_state.outcome.exception()
            message += f"; Exception: {ex.__class__.__name__}: {ex}"
        if retry_state.attempt_number < 1:
            logging.info(message)
        else:
            logging.warning(message)

    # Retry with jitter on transient errors. Initially up to 2^x * 1 seconds between each retry until
    # the range reaches 30 seconds, then randomly up to 60 seconds afterwards. Ultimately, stop after 5 attempts.
    
    @retry(wait=wait_random_exponential(multiplier=1, max=5),
            stop=stop_after_attempt(5),
            before_sleep=__retry_before_sleep)
    def __get_secret_with_retry(client, name):
        try:
            return client.get_secret(name)
        except RetryError:
            pass

   
    def get_keyvault_value(self, name):
        if self.keyvault_name is None:
            self.keyvault_name = self.get_env_var('key_vault_name')

        vault_url = f"https://{self.keyvault_name}.vault.azure.net"

        credential = DefaultAzureCredential()

        client = SecretClient(vault_url=vault_url, credential=credential)

        val = self.__get_secret_with_retry(client, name)

        return val.value

