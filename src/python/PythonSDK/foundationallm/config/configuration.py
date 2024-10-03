import os
import logging
import json
from tenacity import retry, wait_random_exponential, stop_after_attempt, RetryError
from azure.appconfiguration.provider import (
    AzureAppConfigurationKeyVaultOptions,
    load
)
from azure.identity import DefaultAzureCredential

class Configuration():
    def __init__(self):
        """Init"""
        try:
            app_config_uri = os.environ['FOUNDATIONALLM_APP_CONFIGURATION_URI']
        except Exception as e:
            raise e

        credential = DefaultAzureCredential(
            exclude_environment_credential=True)

        # Connect to Azure App Configuration.
        self.__config = load(endpoint=app_config_uri, credential=credential,
                             key_vault_options=
                                AzureAppConfigurationKeyVaultOptions(credential=credential))

    def get_value(self, key: str) -> str:
        """
        Retrieves the value from Azure App Configuration.
        Otherwise, retrieves the value from the environment variable.
        If the value is not found the method raises an exception.

        Parameters
        ----------
        - key : str
            The key name of the configuration setting to retrieve.

        Returns
        -------
        The configuration value

        Raises an exception if the configuration value is not found.
        """
        if key is None:
            raise Exception('The key parameter is required for Configuration.get_value().')

        value = None

        # will have future usage with Azure App Configuration
        # if foundationallm-configuration-allow-environment-variables exists and is True,
        #   then the environment variables will be checked first, then KV
        # if foundationallm-configuration-allow-environment-variables does not exist
        #   OR foundationallm-configuration-allow-environment-variables is False,
        #   then check App config and then KV
        allow_env_vars = False
        if "foundationallm-configuration-allow-environment-variables" in os.environ:
            allow_env_vars = bool(os.environ[
                    "foundationallm-configuration-allow-environment-variables"
                    ])

        if allow_env_vars is True:
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

    def get_feature_flag(self, key: str) -> bool:
        """
        Retrieves the feature flag from Azure App Configuration.
        If the value is not found, returns false.
        Otherwise, retrieves the enabled value of the feature flag.

        Parameters
        ----------
        - key : str
            The key name of the feature flag to retrieve.

        Returns
        -------
        The enabled value of the feature flag

        """
        if key is None:
            raise KeyError('The key parameter is required for Configuration.get_feature_flag().')

        value = False

        if "FeatureManagementFeatureFlags" in self.__config.keys():
            if key in self.__config["FeatureManagementFeatureFlags"].keys():
                try:
                    feature_flag_setting = self.__config["FeatureManagementFeatureFlags"][key]
                    obj = json.loads(feature_flag_setting)
                    value = obj["enabled"]
                except Exception as e:
                    pass

        return value

    def __retry_before_sleep(self, retry_state):
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
