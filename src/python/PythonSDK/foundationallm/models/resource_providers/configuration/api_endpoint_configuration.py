from pydantic import Field
from typing import Any, List, Self, Optional
from foundationallm.models.resource_providers import ResourceBase
from foundationallm.models.resource_providers.configuration import (
    ConfigurationTypes,
    UrlException
)
from foundationallm.utils import ObjectUtils
from foundationallm.langchain.exceptions import LangChainException

class APIEndpointConfiguration(ResourceBase):
    """
    API Endpoint Configuration model.
    """
    type: str = Field(default=ConfigurationTypes.API_ENDPOINT, description="The type of the API endpoint configuration.")
    category: str = Field(description="The category of the API endpoint configuration.")
    authentication_type: str = Field(description="The type of authentication required for accessing the API endpoint.")
    url: str = Field(description="The base URL of the API endpoint.")
    url_exceptions: List[UrlException] = Field(default={}, description="List of URL exceptions for the API endpoint.")
    authentication_parameters: dict = Field(default={}, description="Dictionary with values used for authentication.")
    timeout_seconds: int = Field(default=60, description="The timeout duration in seconds for API calls.")
    retry_strategy_name: str = Field(description="The name of the retry strategy to use for API calls.")
    provider: Optional[str] = Field(default=None, description="The provider of the API endpoint.")
    api_version: Optional[str] = Field(default=None, description="The version to use when calling the API represented by the endpoint.")
    operation_type: Optional[str] = Field(default=None, description="The type of operation the API endpoint is performing.")

    @staticmethod
    def from_object(obj: Any) -> Self:

        endpoint_configuration: APIEndpointConfiguration = None

        try:
            endpoint_configuration = APIEndpointConfiguration(**ObjectUtils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"The API Endpoint Configuration object provided is invalid. {str(e)}", 400)
        
        if endpoint_configuration is None:
            raise LangChainException("The API Endpoint Configuration object provided is invalid.", 400)

        return endpoint_configuration
