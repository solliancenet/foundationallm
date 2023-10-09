import os
from tkinter import N
from azure.keyvault.secrets import SecretClient
from tenacity import (retry, wait_random_exponential, stop_after_attempt, RetryError)
import logging
from foundationallm.auth import Credential

class Configuration():
    def __init__(self, keyvault_name: str = None):
        self.keyvault_name = keyvault_name

        if self.keyvault_name is None:            
            self.keyvault_name = os.environ.get('foundationallm-langchain-api-keyvault-name')
            if self.keyvault_name is None:
                raise Exception('Unable to resolve Key Vault: the keyvault_name parameter was not passed in and the environment variable FLLM_KEYVAULT_NAME is not configured.')
        
        vault_url = f"https://{self.keyvault_name}.vault.azure.net"
        credential = Credential().get_credential()
        self.secret_client = SecretClient(vault_url=vault_url, credential=credential)

    def get_value(self, name: str, default: str = None) -> str:
        """
        Checks if the environment variable exists, if not, retrieves the value from Key Vault.
        If both are not found, returns the default value if provided, otherwise raises an exception.

        Parameters
        ----------
        - name : str
            The name of the env variable to retrieve.
        - default : str
            Default value if variable not found.
        Returns
        -------
        The value of the environment variable if it exists, or the Key Vault
        value for the variable.
        """
        
        value = os.environ.get(name)
        if value is not None:
            return value
        
        try:
            value = self.__get_secret_with_retry(name=name)
            return value
        except Exception as e:            
            pass
        
        if value is not None:
            return value
        # If name not found as an env variable
        else:
            if default:
                return default
            else:
                raise Exception(f'The environment variable {name} does not exist.')

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

    # Retry with jitter on transient errors. Initially up to 2^x * 1 seconds
    # between each retry until the range reaches 30 seconds,
    # then randomly up to 60 seconds afterwards.
    # Ultimately, stop after 5 attempts.
    @retry(
            wait=wait_random_exponential(multiplier=1, max=5),
            stop=stop_after_attempt(5),
            before_sleep=__retry_before_sleep
        )
    def __get_secret_with_retry(self, name):
        try:
            return self.secret_client.get_secret(name).value
        except RetryError:
            pass