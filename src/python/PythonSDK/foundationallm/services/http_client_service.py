import os
import requests
import aiohttp
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.authentication import AuthenticationTypes, AuthenticationParametersKeys
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration

class HttpClientService:
    """
    Class for creating an HTTP client session based on an API endpoint configuration.
    """
    def __init__(self, api_endpoint_configuration: APIEndpointConfiguration, user_identity: UserIdentity, config: Configuration):
        self.config = config
        self.user_identity = user_identity
        self.api_endpoint_configuration = api_endpoint_configuration
        self.base_url = self.api_endpoint_configuration.url.rstrip('/')
        env = os.environ.get('FOUNDATIONALLM_ENV', 'prod')
        self.verify_certs = False if env == 'dev' else True
        self.time_out = self.api_endpoint_configuration.timeout_seconds

        # build headers
        headers = {}
        headers["Content-Type"] = "application/json"
        headers["charset"] = "utf-8"
        if self.api_endpoint_configuration.authentication_type == AuthenticationTypes.API_KEY:
            api_key_value_prefix = self.api_endpoint_configuration.authentication_parameters.get(AuthenticationParametersKeys.API_KEY_PREFIX, "")
            api_key_value = self.config.get_value(self.api_endpoint_configuration.authentication_parameters.get(AuthenticationParametersKeys.API_KEY_CONFIGURATION_NAME))
            api_key_header_name = self.api_endpoint_configuration.authentication_parameters.get(AuthenticationParametersKeys.API_KEY_HEADER_NAME)
            headers[api_key_header_name] = api_key_value_prefix + api_key_value
        else:
            raise Exception(f"Authentication type {self.api_endpoint_configuration.authentication_type} is not currently supported.")

        # Check for base URL exceptions.
        for url_exception in self.api_endpoint_configuration.url_exceptions:
            if url_exception.user_principal_name == self.user_identity.upn and url_exception.enabled:
                self.base_url = url_exception.url.rstrip('/')
                break

        self.headers = headers

    def get(self, endpoint: str):
        """
        Execute a synchronous GET request.
        """
        with requests.Session() as session:
            session.headers.update(self.headers)
            url = self.base_url + endpoint
            response = session.get(url, timeout=self.time_out, verify=self.verify_certs)
            response.raise_for_status()
            return response.json()

    def post(self, endpoint: str, data: dict = None):
        """
        Execute a synchronous POST request.
        """        
        with requests.Session() as session:
            session.headers.update(self.headers)
            url = self.base_url + endpoint            
            response = session.post(url, data=data, timeout=self.time_out, verify=self.verify_certs)            
            response.raise_for_status()
            return response.json()

    async def aget(self, endpoint: str):
        """
        Execute an asynchronous GET request.
        """
        async with aiohttp.ClientSession(headers=self.headers) as session:
            url = self.base_url + endpoint
            async with session.get(url, timeout=self.time_out, ssl=self.verify_certs) as response:
                response.raise_for_status()
                return await response.json()

    async def apost(self, endpoint: str, data: dict = None):
        """
        Execute an asynchronous POST request.
        """
        async with aiohttp.ClientSession(headers=self.headers) as session:
            url = self.base_url + endpoint
            async with session.post(url, data=data, timeout=self.time_out, ssl=self.verify_certs) as response:
                response.raise_for_status()
                return await response.json()
