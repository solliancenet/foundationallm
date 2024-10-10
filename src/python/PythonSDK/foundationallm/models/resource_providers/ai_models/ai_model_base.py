from pydantic import Field
from typing import Any, Self, Optional
from foundationallm.models.resource_providers import ResourceBase
from foundationallm.utils import ObjectUtils
from foundationallm.langchain.exceptions import LangChainException

class AIModelBase(ResourceBase):
    """
    The base class used for AIModel resources.
    """
    endpoint_object_id: str = Field(description="The object ID of the APIEndpointConfiguration object providing the configuration for the API endpoint used to interact with the model.")
    version: Optional[str] = Field(description="The version of the AI model.")
    deployment_name: Optional[str] = Field(description="The deployment name for the AI model.")
    model_parameters: Optional[dict] = Field(default={}, description="A dictionary containing default values for model parameters.")

    @staticmethod
    def from_object(obj: Any) -> Self:

        ai_model: AIModelBase = None

        try:
            ai_model = AIModelBase(**ObjectUtils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"The AI model object provided is invalid. {str(e)}", 400)
        
        if ai_model is None:
            raise LangChainException("The AI model object provided is invalid.", 400)

        return ai_model
