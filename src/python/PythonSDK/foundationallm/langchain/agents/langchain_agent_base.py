from abc import abstractmethod
from typing import List
from azure.identity import DefaultAzureCredential, get_bearer_token_provider

from langchain_core.language_models import BaseLanguageModel
from langchain_openai import AzureChatOpenAI, ChatOpenAI, OpenAI
from openai import AzureOpenAI as aoi
from openai import AsyncAzureOpenAI as async_aoi
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.operations import OperationsManager
from foundationallm.models.authentication import AuthenticationTypes
from foundationallm.models.language_models import LanguageModelProvider
from foundationallm.models.orchestration import (
    CompletionRequestBase,
    CompletionResponse,
    MessageHistoryItem,
    OperationTypes
)
from foundationallm.models.resource_providers.ai_models import AIModelBase
from foundationallm.models.resource_providers.attachments import Attachment
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.models.resource_providers.prompts import MultipartPrompt

class LangChainAgentBase():
    """
    Implements the base functionality for a LangChain agent.
    """
    def __init__(self, instance_id: str, user_identity: UserIdentity, config: Configuration, operations_manager: OperationsManager):
        """
        Initializes a knowledge management agent.

        Parameters
        ----------
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.instance_id = instance_id
        self.user_identity = user_identity
        self.config = config
        self.ai_model = None
        self.api_endpoint = None
        self.prompt = ''
        self.full_prompt = ''
        self.has_indexing_profiles = False
        self.has_retriever = False
        self.operations_manager = operations_manager

    @abstractmethod
    def invoke(self, request: CompletionRequestBase) -> CompletionResponse:
        """
        Gets the completion for the request.
        
        Parameters
        ----------
        request : CompletionRequestBase
            The completion request to execute.

        Returns
        -------
        CompletionResponse
            Returns a completion response.
        """
        raise NotImplementedError()

    @abstractmethod
    async def ainvoke(self, request: CompletionRequestBase) -> CompletionResponse:
        """
        Gets the completion for the request using an async request.
        
        Parameters
        ----------
        request : CompletionRequestBase
            The completion request to execute.

        Returns
        -------
        CompletionResponse
            Returns a completion response.
        """
        raise NotImplementedError()

    @abstractmethod
    def _validate_request(self, request: CompletionRequestBase):
        """
        Validates that the completion request contains all required properties.

        Parameters
        ----------
        request : CompletionRequestBase
            The completion request to validate.
        """
        raise NotImplementedError()

    def _get_prompt_from_object_id(self, prompt_object_id: str, objects: dict) -> MultipartPrompt:
        """
        Get the prompt from the object id.
        """
        prompt: MultipartPrompt = None

        if prompt_object_id is None or prompt_object_id == '':
            raise LangChainException("Invalid prompt object id.", 400)
        
        try:
            prompt = MultipartPrompt(**objects.get(prompt_object_id))
        except Exception as e:
            raise LangChainException(f"The prompt object provided in the request.objects dictionary is invalid. {str(e)}", 400)
        
        if prompt is None:
            raise LangChainException("The prompt object is missing in the request.objects dictionary.", 400)

        return prompt

    def _get_ai_model_from_object_id(self, ai_model_object_id: str, objects: dict) -> AIModelBase:
        """
        Get the AI model from its object id.
        """
        ai_model: AIModelBase = None

        if ai_model_object_id is None or ai_model_object_id == '':
            raise LangChainException("Invalid AI model object id.", 400)
        
        try:
            ai_model = AIModelBase(**objects.get(ai_model_object_id))
        except Exception as e:
            raise LangChainException(f"The AI model object provided in the request.objects dictionary is invalid. {str(e)}", 400)
        
        if ai_model is None:
            raise LangChainException("The AI model object is missing in the request.objects dictionary.", 400)

        return ai_model

    def _get_api_endpoint_from_object_id(self, api_endpoint_object_id: str, objects: dict) -> APIEndpointConfiguration:
        """
        Get the API endpoint from its object id.
        """
        api_endpoint: APIEndpointConfiguration = None

        if api_endpoint_object_id is None or api_endpoint_object_id == '':
            raise LangChainException("Invalid API endpoint object id.", 400)
        
        try:
            api_endpoint = APIEndpointConfiguration(**objects.get(api_endpoint_object_id))
        except Exception as e:
            raise LangChainException(f"The API endpoint object provided in the request.objects dictionary is invalid. {str(e)}", 400)
        
        if api_endpoint is None:
            raise LangChainException("The API endpoint object is missing in the request.objects dictionary.", 400)

        return api_endpoint

    def _get_attachment_from_object_id(self, attachment_object_id: str, agent_parameters: dict) -> Attachment:
        """
        Get the attachment from its object id.
        """
        attachment: Attachment = None

        if attachment_object_id is None or attachment_object_id == '':
            return None
        
        try:
            attachment = Attachment(**agent_parameters.get(attachment_object_id))
        except Exception as e:
            raise LangChainException(f"The attachment object provided in the agent parameters is invalid. {str(e)}", 400)
        
        if attachment is None:
            raise LangChainException("The attachment object is missing in the agent parameters.", 400)

        return attachment        

    def _build_conversation_history(self, messages:List[MessageHistoryItem]=None, message_count:int=None) -> str:
        """
        Builds a chat history string from a list of MessageHistoryItem objects to
        be added to the prompt for the completion request.

        Parameters
        ----------
        messages : List[MessageHistoryItem]
            The list of messages from which to build the chat history.
        message_count : int
            The number of messages to include in the chat history.
        """
        if messages is None or len(messages)==0:
            return ""
        if message_count is not None:
            messages = messages[-message_count:]
        chat_history = "Chat History:\n"
        for msg in messages:
            chat_history += msg.sender + ": " + msg.text + "\n"
        chat_history += "\n\n"
        return chat_history

    def _record_full_prompt(self, prompt: str) -> str:
        """
        Records the full prompt for the completion request.

        Parameters
        ----------
        prompt : str
            The prompt that is populated with context.
        
        Returns
        -------
        str
            Returns the full prompt.
        """
        self.full_prompt = prompt
        return prompt

    def _get_language_model(self, override_operation_type: OperationTypes = None, is_async: bool=False) -> BaseLanguageModel:
        """
        Create a language model using the specified endpoint settings.

        override_operation_type : OperationTypes - internally override the operation type for the API endpoint.

        Returns
        -------
        BaseLanguageModel
            Returns an API connector for a chat completion model.
        """                
        language_model:BaseLanguageModel = None
        api_key = None

        if self.ai_model is None:
            raise LangChainException("AI model configuration settings are missing.", 400)
        if self.api_endpoint is None:
            raise LangChainException("API endpoint configuration settings are missing.", 400)

        if self.api_endpoint.provider == LanguageModelProvider.MICROSOFT:
            op_type = self.api_endpoint.operation_type
            if override_operation_type is not None:
                op_type = override_operation_type
            if self.api_endpoint.authentication_type == AuthenticationTypes.AZURE_IDENTITY:
                try:
                    scope = self.api_endpoint.authentication_parameters.get('scope', 'https://cognitiveservices.azure.com/.default')
                    # Set up a Azure AD token provider.
                    # TODO: Determine if there is a more efficient way to get the token provider than making the request for every call.
                    token_provider = get_bearer_token_provider(
                        DefaultAzureCredential(exclude_environment_credential=True),
                        scope
                    )
                    
                    if op_type == OperationTypes.CHAT:
                        language_model = AzureChatOpenAI(
                            azure_endpoint=self.api_endpoint.url,
                            api_version=self.api_endpoint.api_version,
                            openai_api_type='azure_ad',
                            azure_ad_token_provider=token_provider,
                            azure_deployment=self.ai_model.deployment_name
                        )
                    elif op_type == OperationTypes.ASSISTANTS_API or op_type == OperationTypes.IMAGE_ANALYSIS:
                        # Assistants API clients can't have deployment as that is assigned at the assistant level.
                        if is_async:
                            # create async client
                            language_model = async_aoi(
                                azure_endpoint=self.api_endpoint.url,
                                api_version=self.api_endpoint.api_version,
                                openai_api_type='azure_ad',
                                azure_ad_token_provider=token_provider,
                            )
                        else:
                            language_model = aoi(
                                azure_endpoint=self.api_endpoint.url,
                                api_version=self.api_endpoint.api_version,
                                openai_api_type='azure_ad',
                                azure_ad_token_provider=token_provider,
                            )
                    else:
                        raise LangChainException(f"Unsupported operation type: {op_type}", 400)

                except Exception as e:
                    raise LangChainException(f"Failed to create Azure OpenAI API connector: {str(e)}", 500)
            else: # Key-based authentication
                try:
                    api_key = self.config.get_value(self.api_endpoint.authentication_parameters.get('api_key_configuration_name'))
                except Exception as e:
                    raise LangChainException(f"Failed to retrieve API key: {str(e)}", 500)

                if api_key is None:
                    raise LangChainException("API key is missing from the configuration settings.", 400)
                        
                if op_type == OperationTypes.CHAT:
                    language_model = AzureChatOpenAI(
                        azure_endpoint=self.api_endpoint.url,
                        api_key=api_key,
                        api_version=self.api_endpoint.api_version,
                        azure_deployment=self.ai_model.deployment_name
                    )
                elif op_type == OperationTypes.ASSISTANTS_API or op_type == OperationTypes.IMAGE_ANALYSIS:
                    # Assistants API clients can't have deployment as that is assigned at the assistant level.
                    if is_async:
                        # create async client
                        language_model = async_aoi(
                            azure_endpoint=self.api_endpoint.url,
                            api_key=api_key,
                            api_version=self.api_endpoint.api_version,
                        )
                    else:
                        language_model = aoi(
                            azure_endpoint=self.api_endpoint.url,
                            api_key=api_key,
                            api_version=self.api_endpoint.api_version,
                        )
                else:
                    raise LangChainException(f"Unsupported operation type: {op_type}", 400)

        else:
            try:
                api_key = self.config.get_value(self.api_endpoint.authentication_parameters.get('api_key_configuration_name'))
            except Exception as e:
                raise LangChainException(f"Failed to retrieve API key: {str(e)}", 500)

            if api_key is None:
                raise LangChainException("API key is missing from the configuration settings.", 400)
                
            language_model = (
                ChatOpenAI(base_url=self.api_endpoint.url, api_key=api_key)
                if self.api_endpoint.operation_type == OperationTypes.CHAT
                else OpenAI(base_url=self.api_endpoint.url, api_key=api_key)
            )

        # Set model parameters.
        for key, value in self.ai_model.model_parameters.items():
            if hasattr(language_model, key):
                setattr(language_model, key, value)

        return language_model
