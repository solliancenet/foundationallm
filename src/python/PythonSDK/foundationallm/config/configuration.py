import os
#from re import A
#from tkinter import N
from azure.keyvault.secrets import SecretClient
#from sqlalchemy import exc
from tenacity import retry, wait_random_exponential, stop_after_attempt, RetryError
import logging
from azure.appconfiguration.provider import (
    AzureAppConfigurationKeyVaultOptions,
    load,
    #SettingSelector
)
from azure.identity import DefaultAzureCredential

class Configuration():    
    def __init__(self):
        """Init"""
        try:
            vault_url = os.environ['foundationallm-keyvault-uri']
        except Exception as e:
            raise e
        
        credential = DefaultAzureCredential()
        secret_client = SecretClient(vault_url=vault_url, credential=credential)
        app_config_secret_name = os.environ.get("foundationallm-app-configuration-secret-name")
        connection_string = secret_client.get_secret(name=app_config_secret_name).value
        
        # Connect to Azure App Configuration using a connection string.
        self.__config = load(connection_string=connection_string, key_vault_options=AzureAppConfigurationKeyVaultOptions(credential=credential))
            
    def get_value(self, key: str) -> str:
        """
        When prefer_secrets_store is true, retrieves the value from Key Vault.
        Otherwise, retrieves the value from the environment variable.
        If the value is not found the method raises an exception.

        Parameters
        ----------
        - name : str
            The name of the configuration variable to retrieve.
        
        Returns
        -------
        The configuration value

        Raises an exception if the configuration value is not found.
        """
        if key is None:
            raise Exception('The key parameter is required for Configuration.get_value().')
        
        value = None
        
        # will have future usage with Azure App Configuration
        # if foundationallm-configuration-allow-environment-variables exists and is True, then the environment variables will be checked first, then KV
        # if foundationallm-configuration-allow-environment-variables does not exist OR foundationallm-configuration-allow-environment-variables is False, then check App config and then KV
        allow_env_vars = False
        if "foundationallm-configuration-allow-environment-variables" in os.environ:
            allow_env_vars = bool(os.environ["foundationallm-configuration-allow-environment-variables"])
               
        if allow_env_vars == True:
            value = os.environ.get(key)
            
        if value is None:               
            try:
                value = self.__get_config_with_retry(name=key)                
            except Exception as e:            
                pass

        if value is not None:
            return value        
        else:           
            raise Exception(f'The configuration variable {key} was not found.')

    def __retry_before_sleep(retry_state):
        # Log the outcome of each retry attempt.
        message = f"""Retrying {retry_state.fn}:
                        attempt {retry_state.attempt_number}
                        ended with: {retry_state.outcome}"""
        if retry_state.outcome.failed:
            ex = retry_state.outcome.exception()
            message += f"; Exception: {ex.__class__.__name__}: {ex}"
        if retry_state.attempt_number < 1:
            logging.info(message)
        else:
            logging.warning(message)

    # Retry with jitter on transient errors. Initially up to 2^x * 1 seconds between each retry
    # until the range reaches 30 seconds, then randomly up to 60 seconds afterwards.
    # Stop after five retry attempts.
    @retry(
        wait=wait_random_exponential(multiplier=1, max=5),
        stop=stop_after_attempt(5),
        before_sleep=__retry_before_sleep
    )
    def __get_config_with_retry(self, name):
        try:
            return self.__config[name]
        except RetryError:
            pass