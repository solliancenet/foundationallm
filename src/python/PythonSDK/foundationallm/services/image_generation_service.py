import base64
import json
from foundationallm.models.attachments import AttachmentProperties
from foundationallm.config import Configuration
from foundationallm.storage import BlobStorageManager
from openai import AzureOpenAI, AsyncAzureOpenAI
from openai.types import CompletionUsage
from typing import List, Union

class ImageGenerationService:
    """
    Performs image generation via the Azure OpenAI SDK.
    """
    def __init__(self, config: Configuration, client: Union[AzureOpenAI, AsyncAzureOpenAI], deployment_model: str):
        """
        Initializes the ImageGenerationService.

        Parameters
        ----------
        config : Configuration
            Application configuration class for retrieving configuration settings.
        client : Union[AzureOpenAI, AsyncAzureOpenAI]
            The Azure OpenAI client to use for image generation.
        deployment_model : str
            The deployment model to use for the Azure OpenAI client.
        """
        self.config = config
        self.client = client
        self.deployment_model = deployment_model

    def generate_image(self, prompt:str):
        pass
