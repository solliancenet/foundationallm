import base64
import json
from foundationallm.models.attachments import AttachmentProperties
from foundationallm.config import Configuration
from foundationallm.storage import BlobStorageManager
from openai import AzureOpenAI, AsyncAzureOpenAI
from openai.types import CompletionUsage
from typing import List, Union

from foundationallm.telemetry import Telemetry

# Initialize telemetry logging
logger = Telemetry.get_logger(__name__)
tracer = Telemetry.get_tracer(__name__)

class ImageAnalysisService:
    """
    Performs image analysis via the Azure OpenAI SDK.
    """
    def __init__(self, config: Configuration, client: Union[AzureOpenAI, AsyncAzureOpenAI], deployment_model: str):
        """
        Initializes the ImageAnalysisService.

        Parameters
        ----------
        config : Configuration
            Application configuration class for retrieving configuration settings.
        client : Union[AzureOpenAI, AsyncAzureOpenAI]
            The Azure OpenAI client to use for image analysis.
        deployment_model : str
            The deployment model to use for the Azure OpenAI client.
        """
        self.config = config
        self.client = client
        self.deployment_model = deployment_model

    def _get_as_base64(self, mime_type: str, storage_account_name, file_path: str) -> str:
        """
        Retrieves an image from its URL and converts it to a base64 string.

        Parameters
        ----------
        mime_type : str
            The mime type of the image.
        image_url : str
            The URL of the image.

        Returns
        -------
        str
            The image as a base64 string.
        """
        try:
            # Remove any leading slashes from the file path.
            file_path = file_path.lstrip('/')
            # Attempt to retrieve the image from blob storage.
            container_name = file_path.split('/')[0]
            # Get the file path without the container name.
            file_name = file_path.removeprefix(container_name)

            try:
                storage_manager = BlobStorageManager(
                    account_name=storage_account_name,
                    container_name=container_name,
                    authentication_type=self.config.get_value('FoundationaLLM:ResourceProviders:Attachment:Storage:AuthenticationType')
                )
            except Exception as e:
                raise Exception(f'Error connecting to the {storage_account_name} blob storage account and the container named {container_name}: {e}')

            if (storage_manager.file_exists(file_name)):
                try:
                    # Get the image file from blob storage.
                   image_blob = storage_manager.read_file_content(file_name)
                   return base64.b64encode(image_blob).decode('utf-8')
                except Exception as e:
                    raise Exception(f'The specified image {storage_account_name}/{file_path} does not exist.')
            else:
                raise Exception(f'The specified image {storage_account_name}/{file_path} does not exist.')
        except Exception as e:
            logger.error(f'Error getting image as base64: {e}')
            return None

    def format_results(self, image_analyses: dict) -> str:
        """
        Formats the image analysis results into a markdown table.

        Parameters
        ----------
        image_analyses : dict
            The dictionary containing the image analysis results.

        Returns
        -------
        str
            The formatted image analysis results.
        """
        formatted_results = f"You have access to the following {len(image_analyses)} images and their analysis results:\n"
        for idx, key in enumerate(image_analyses):
            formatted_results += f"## Image {idx + 1}:\n"
            formatted_results += f"- Name : {key}\n"
            formatted_results += f"- Analysis: {image_analyses[key]}\n\n"
        return formatted_results

    async def aanalyze_images(self, image_attachments: List[AttachmentProperties]) -> tuple:
        """
        Get the image analysis results from Azure OpenAI.

        Parameters
        ----------
        image_attachments : List[AttachmentProperties]
            The list containing properties of the images to analyze.
        """
        image_analyses = {}
        usage = CompletionUsage(completion_tokens=0, prompt_tokens=0, total_tokens=0)

        for attachment in image_attachments:
            if attachment.content_type.startswith('image/'):
                image_base64 = self._get_as_base64(mime_type=attachment.content_type, storage_account_name=attachment.provider_storage_account_name, file_path=attachment.provider_file_name)
                if image_base64 is not None and image_base64 != '':
                    response = await self.client.chat.completions.create(
                        model=self.deployment_model,
                        messages=[
                            {
                                "role": "system",
                                "content": "You are a helpful assistant who analyzes and describes images. Provide as many key insights and analysis about the data in the image as possible. Output the results in a markdown formatted table."
                            },
                            {
                                "role": "user",
                                "content": [
                                    {
                                        "type": "text",
                                        "content": "Analyze the image:"
                                    },
                                    {
                                        "type": "image_url",
                                        "image_url": {
                                            "url": f"data:{attachment.content_type};base64,{image_base64}"
                                        }
                                    }
                                ]
                            }
                        ],
                        max_tokens=4000,
                        temperature=0.5
                    )
                    image_analyses[attachment.original_file_name] = response.choices[0].message.content
                    usage.prompt_tokens += response.usage.prompt_tokens
                    usage.completion_tokens += response.usage.completion_tokens
                    usage.total_tokens += response.usage.total_tokens
                else:
                    image_analyses[attachment.original_file_name] = f"The image {attachment.original_file_name} was either invalid or inaccessible and could not be analyzed."

        return image_analyses, usage

    def analyze_images(self, image_attachments: List[AttachmentProperties]) -> tuple:
        """
        Get the image analysis results from Azure OpenAI.

        Parameters
        ----------
        image_attachments : List[AttachmentProperties]
            The list containing properties of the images to analyze.
        """
        image_analyses = {}
        usage = CompletionUsage(completion_tokens=0, prompt_tokens=0, total_tokens=0)

        for attachment in image_attachments:
            if attachment.content_type.startswith('image/'):
                image_base64 = self._get_as_base64(mime_type=attachment.content_type, storage_account_name=attachment.provider_storage_account_name, file_path=attachment.provider_file_name)
                if image_base64 is not None and image_base64 != '':
                    response = self.client.chat.completions.create(
                        model=self.deployment_model,
                        messages=[
                            {
                                "role": "system",
                                "content": "You are a helpful assistant who analyzes and describes images. Provide as many key insights and analysis about the data in the image as possible. Output the results in a markdown formatted table."
                            },
                            {
                                "role": "user",
                                "content": [
                                    {
                                        "type": "text",
                                        "content": "Analyze the image:"
                                    },
                                    {
                                        "type": "image_url",
                                        "image_url": {
                                            "url": f"data:{attachment.content_type};base64,{image_base64}"
                                        }
                                    }
                                ]
                            }
                        ],
                        max_tokens=4000,
                        temperature=0.5
                    )
                    image_analyses[attachment.original_file_name] = response.choices[0].message.content
                    usage.prompt_tokens += response.usage.prompt_tokens
                    usage.completion_tokens += response.usage.completion_tokens
                    usage.total_tokens += response.usage.total_tokens
                else:
                    image_analyses[attachment.original_file_name] = f"The image {attachment.original_file_name} was either invalid or inaccessible and could not be analyzed."

        return image_analyses, usage
