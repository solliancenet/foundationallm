import re
from abc import abstractmethod
from typing import List, Optional
from azure.identity import DefaultAzureCredential, get_bearer_token_provider

from langchain_core.language_models import BaseLanguageModel
from langchain_openai import AzureChatOpenAI, AzureOpenAI, ChatOpenAI, OpenAI
from langchain_community.chat_models.azureml_endpoint import AzureMLChatOnlineEndpoint
from langchain_community.chat_models.azureml_endpoint import (
    AzureMLEndpointApiType,
    CustomOpenAIChatContentFormatter,
)
from foundationallm.config.configuration import Configuration
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.models.orchestration import OperationTypes
from foundationallm.models.agents import AgentBase
from foundationallm.models.authentication import AuthenticationTypes
from foundationallm.models.language_models import LanguageModelProvider
from foundationallm.models.orchestration import (
    CompletionRequestBase,
    CompletionResponse,
    EndpointSettings,
    MessageHistoryItem,
    OrchestrationSettings   
)
from foundationallm.models.resource_providers.attachments import Attachment
from foundationallm.models.resource_providers.prompts import MultipartPrompt
from foundationallm.models.resource_providers.vectorization import (
    AzureAISearchIndexingProfile,
    AzureOpenAIEmbeddingProfile
)

class LangChainAgentBase():
    """
    Implements the base functionality for a LangChain agent.
    """
    def __init__(self, config: Configuration):
        """
        Initializes a knowledge management agent.

        Parameters
        ----------
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.config = config
        self.full_prompt = ""

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

    def _get_prompt_from_object_id(self, prompt_object_id: str, agent_parameters: dict) -> MultipartPrompt:
        """
        Get the prompt from the object id.
        """
        prompt: MultipartPrompt = None

        if prompt_object_id is None or prompt_object_id == '':
            raise LangChainException("Invalid prompt object id.", 400)
        
        try:
            prompt = MultipartPrompt(**agent_parameters.get(prompt_object_id))
        except Exception as e:
            raise LangChainException(f"The prompt object provided in the agent parameters is invalid. {str(e)}", 400)
        
        if prompt is None:
            raise LangChainException("The prompt object is missing in the agent parameters.", 400)

        return prompt

    def _get_attachment_from_object_id(self, attachment_object_id: str, agent_parameters: dict) -> Attachment:
        """
        Get the prompt from the object id.
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

    def _validate_request(self, request: CompletionRequestBase):
        """
        Validates that the completion request's agent contains all require properties.

        Parameters
        ----------
        request : CompletionRequestBase
            The completion request to validate.
        """
        if request.agent is None:
            raise LangChainException("The Agent property of the completion request cannot be null.", 400)
        
        if request.agent.orchestration_settings is None:
            raise LangChainException("The OrchestrationSettings property of the agent cannot be null.", 400)
        
        if request.agent.orchestration_settings.endpoint_configuration is None:
            raise LangChainException("The EndpointConfiguration property of the OrchestrationSettings cannot be null.", 400)
        
        if request.agent.orchestration_settings.endpoint_configuration.get('endpoint') is None:
            raise LangChainException("The Endpoint property of the agent's OrchestrationSettings.EndpointConfiguration property cannot be null.", 400)

        autentication_type = request.agent.orchestration_settings.endpoint_configuration.get('auth_type')
        if autentication_type is None:
            raise LangChainException("The AuthType property of the agent's OrchestrationSettings.EndpointConfiguration property cannot be null.", 400)

        try:
            AuthenticationTypes(autentication_type)
        except ValueError:
            raise LangChainException(f"The authentication type {autentication_type} is not supported.", 400)
        
        provider = request.agent.orchestration_settings.endpoint_configuration.get('provider')
        if provider is None:
            raise LangChainException("The Provider property of the agent's OrchestrationSettings.EndpointConfiguration property cannot be null.", 400)

        try:
            LanguageModelProvider(provider)
        except ValueError:
            raise LangChainException(f"The LLM provider {provider} is not supported.", 400)

        match provider:
            case LanguageModelProvider.MICROSOFT:
                # Verify the endpoint_configuration inludes the api_version property for Azure OpenAI models.
                if request.agent.orchestration_settings.endpoint_configuration.get('api_version') is None:
                    raise LangChainException("The ApiVersion property of the agent's OrchestrationSettings.EndpointConfiguration property cannot be null.", 400)
            
                # model_parameters is required to provide the deployment_name for Azure OpenAI models.
                if request.agent.orchestration_settings.model_parameters is None:
                    raise LangChainException("The ModelParameters property of the OrchestrationSettings cannot be null.", 400)

                # Verify that the deployment_name is provided for Azure OpenAI models.
                if request.agent.orchestration_settings.model_parameters.get('deployment_name') is None:
                    raise LangChainException("The DeploymentName property of the agent's OrchestrationSettings.ModelParameters property cannot be null.", 400)
            case LanguageModelProvider.AZUREML:
                # Verify the endpoint_configuration inludes the endpoint property for AzureML models.
                if request.agent.orchestration_settings.endpoint_configuration.get('endpoint') is None:
                    raise LangChainException("The Endpoint property of the agent's OrchestrationSettings.EndpointConfiguration property cannot be null.", 400)
                # Verify the endpoint_api_key property for AzureML models.
                if request.agent.orchestration_settings.endpoint_configuration.get('api_key') is None:
                    raise LangChainException("The APIKey property of the agent's OrchestrationSettings.EndpointConfiguration property cannot be null.", 400)

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

    def __extract_endpoint_configuration(
            self,
            endpoint_configuration: dict) -> EndpointSettings:
        """
        Extracts the endpoint configuration settings from the agent's orchestration settings.

        Parameters
        ----------
        config : Configuration
            Application configuration class for retrieving configuration settings.
        endpoint_configuration : dict
            The endpoint configuration settings to extract.

        Returns
        -------
        EndpointSettings
            Returns the endpoint settings for the completion request.
        """
        # TODO: Also allow for the override of the endpoint configuration?
        endpoint_settings = EndpointSettings(
            endpoint=endpoint_configuration.get('endpoint'),
            authentication_type=endpoint_configuration.get('auth_type'),
            provider=endpoint_configuration.get('provider'),
            api_version=endpoint_configuration.get('api_version'),
            operation_type=endpoint_configuration.get('operation_type') or 'chat'            
        )

        endpoint_settings.api_type = 'azure_ad' if endpoint_settings.authentication_type == 'token' else 'azure'

        if endpoint_settings.authentication_type == AuthenticationTypes.KEY:
            endpoint_settings.api_key = self.config.get_value(endpoint_configuration.get('api_key'))
            if endpoint_settings.api_key is None:
                raise ValueError(f"API Key is required for completion requests using {endpoint_settings.authentication_type}-based authentication.")

        return endpoint_settings
        

    def _get_language_model(
            self,
            agent_orchestration_settings: OrchestrationSettings,
            model_override_settings: Optional[OrchestrationSettings] = None) -> BaseLanguageModel:
        """
        Create a language model using the specified endpoint settings.

        Parameters
        ----------
        config : Configuration
            Application configuration class for retrieving configuration settings.
        agent_orchestration_settings : OrchestrationSettings
            The settings for the completion request configured on the agent.
        model_override_settings : Optional[OrchestrationSettings]
            Any settings specified on the completion request for overriding the model's settings.

        Returns
        -------
        BaseLanguageModel
            Returns an API connector for a chat completion model.
        """        
        endpoint_settings = self.__extract_endpoint_configuration(agent_orchestration_settings.endpoint_configuration)
        language_model:BaseLanguageModel = None

        match endpoint_settings.provider:
            case LanguageModelProvider.MICROSOFT:
                # Get Azure OpenAI Chat model settings
                deployment_name = (model_override_settings.model_parameters.get('deployment_name')
                                    if model_override_settings is not None
                                        and model_override_settings.model_parameters is not None
                                        and model_override_settings.model_parameters.get('deployment_name') is not None
                                    else agent_orchestration_settings.model_parameters.get('deployment_name'))
                if deployment_name is None:
                    raise ValueError("Deployment name is required for Azure OpenAI completion requests.")

                if endpoint_settings.authentication_type == AuthenticationTypes.TOKEN:
                    try:
                        # Set up a Azure AD token provider.
                        # TODO: Determine if there is a more efficient way to get the token provider than making the request for every call.
                        token_provider = get_bearer_token_provider(
                            DefaultAzureCredential(exclude_environment_credential=True),
                            'https://cognitiveservices.azure.com/.default'
                        )
                
                        language_model = (
                            AzureChatOpenAI(
                                azure_endpoint=endpoint_settings.endpoint,
                                api_version=endpoint_settings.api_version,
                                openai_api_type=endpoint_settings.api_type,
                                azure_ad_token_provider=token_provider,
                                azure_deployment=deployment_name
                            ) if endpoint_settings.operation_type == OperationTypes.CHAT
                            else AzureOpenAI(
                                azure_endpoint=endpoint_settings.endpoint,
                                api_version=endpoint_settings.api_version,
                                openai_api_type=endpoint_settings.api_type,
                                azure_ad_token_provider=token_provider,
                                azure_deployment=deployment_name
                            )
                        )
                    except Exception as e:
                        raise LangChainException(f"Failed to create Azure OpenAI API connector: {str(e)}", 500)
                else: # Key-based authentication
                    language_model = (
                        AzureChatOpenAI(
                            azure_endpoint=endpoint_settings.endpoint,
                            api_key=endpoint_settings.api_key,
                            api_version=endpoint_settings.api_version,
                            azure_deployment=deployment_name
                        ) if endpoint_settings.operation_type == OperationTypes.CHAT
                        else AzureOpenAI(
                            azure_endpoint=endpoint_settings.endpoint,
                            api_key=endpoint_settings.api_key,
                            api_version=endpoint_settings.api_version,
                            azure_deployment=deployment_name
                        )
                    )
            case LanguageModelProvider.AZUREML:
                # Overrides are handled in the model_kwargs parameter for AzureML models.
                model_kwargs = agent_orchestration_settings.model_parameters;
                # Override model parameters from completion request settings, if any exist.
                if model_override_settings is not None and model_override_settings.model_parameters is not None:            
                    for key, value in model_override_settings.model_parameters.items():
                        if hasattr(model_kwargs, key):
                            setattr(model_kwargs, key, value)
                
                language_model = AzureMLChatOnlineEndpoint(
                    endpoint_url=endpoint_settings.endpoint,
                    endpoint_api_key=endpoint_settings.api_key,
                    endpoint_api_type=AzureMLEndpointApiType.dedicated,
                    content_formatter=CustomOpenAIChatContentFormatter(),
                    model_kwargs=model_kwargs                    
                )                
            case _:
                language_model = (
                    ChatOpenAI(base_url=endpoint_settings.endpoint, api_key=endpoint_settings.api_key)
                    if endpoint_settings.operation_type == OperationTypes.CHAT
                    else OpenAI(base_url=endpoint_settings.endpoint, api_key=endpoint_settings.api_key)
                )
        
        # Set model parameters from agent orchestration settings.
        if agent_orchestration_settings.model_parameters is not None:
            for key, value in agent_orchestration_settings.model_parameters.items():
                if hasattr(language_model, key):
                    setattr(language_model, key, value)
                 
        # Override model parameters from completion request settings, if any exist.
        if model_override_settings is not None and model_override_settings.model_parameters is not None:            
            for key, value in model_override_settings.model_parameters.items():
                if hasattr(language_model, key):
                    setattr(language_model, key, value)
                               
        return language_model
