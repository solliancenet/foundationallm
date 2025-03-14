"""
Base class for FoundationaLLM tools that implement intent-based invocation
with the FoundationaLLMAgentRouterWorkflow workflow.
"""

# Platform imports
from typing import List, Dict, Tuple

#Azure imports
from azure.identity import DefaultAzureCredential, get_bearer_token_provider

# LangChain imports
from langchain_core.messages import (
    BaseMessage,
    SystemMessage,
    HumanMessage
)
from langchain_core.runnables import RunnableConfig
from langchain_openai import AzureChatOpenAI

from opentelemetry.trace import SpanKind

# FoundationaLLM imports
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.models.authentication import AuthenticationTypes
from foundationallm.models.agents import AgentTool
from foundationallm.models.constants import (
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,
    AIModelResourceTypeNames,
    PromptResourceTypeNames
)
from foundationallm.models.orchestration import ContentArtifact

from foundationallm_agent_plugins.common.constants import (
    CONTENT_ARTIFACT_TYPE_TOOL_EXECUTION,
    CONTENT_ARTIFACT_TYPE_TOOL_ERROR
)

class FoundationaLLMIntentToolBase(FoundationaLLMToolBase):
    """
    Provides a base implementation for tools intented to work
    with the AzureOpenAIRouterWorkflow workflow.
    """

    def __init__(
        self,
        tool_config: AgentTool,
        objects: Dict,
        user_identity:UserIdentity,
        config: Configuration):
        """ Initializes the FoundationaLLMIntentToolBase class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """

        super().__init__(tool_config, objects, user_identity, config)

        self.default_error_message = tool_config.properties.get(
            'default_error_message',
            'An error occurred while processing the request.') \
            if tool_config.properties else 'An error occurred while processing the request.'

        self.default_credential = DefaultAzureCredential(exclude_environment_credential=True)
        self.main_llm_deployment_name = None

        self.__create_main_llm()
        self.__create_main_prompt()

    def _run(
        self,
        *args,
        **kwargs
        ) -> str:

        raise NotImplementedError()

    async def _arun(
        self,
        *args,
        prompt: str = None,
        intents: List[Dict] = None,
        message_history: List[BaseMessage] = None,
        runnable_config: RunnableConfig = None,
        **kwargs,
        ) -> str:

        prompt_tokens = 0
        completion_tokens = 0

        # Get the original prompt
        if runnable_config is None:
            original_prompt = prompt
        else:
            original_prompt = runnable_config['configurable']['original_user_prompt']

        messages = [
            SystemMessage(content=self.main_prompt),
            *message_history,
            HumanMessage(content=original_prompt)
        ]

        with self.tracer.start_as_current_span(self.name, kind=SpanKind.INTERNAL):
            try:

                with self.tracer.start_as_current_span(f'{self.name}_initial_llm_call', kind=SpanKind.INTERNAL):
                    
                    response = await self.main_llm.ainvoke(messages)

                    completion_tokens += response.usage_metadata['input_tokens']
                    prompt_tokens += response.usage_metadata['output_tokens']

                    return response.content, \
                        [
                            self.create_tool_content_artifact(
                                original_prompt,
                                prompt,
                                prompt_tokens,
                                completion_tokens
                            )
                        ]

            except Exception as e:
                self.logger.error('An error occured in tool %s: %s', self.name, e)
                return self.default_error_message, \
                    [
                        self.create_tool_content_artifact(
                            original_prompt,
                            prompt,
                            prompt_tokens,
                            completion_tokens
                        ),
                        self.create_error_content_artifact(
                            original_prompt,
                            e
                        )
                    ]

    async def invoke_with_intents_async(
            self,
            user_prompt: str,
            intents: List,
            message_history: List[BaseMessage]) -> Tuple[str, List[ContentArtifact]]:
        """
        Implements the invocation pattern required by the FoundationaLLMAgentRouterWorkflow workflow.

        Parameters
        ----------
        user_prompt : str
            The user prompt message.
        intents : List
            The list of intents to process.
        message_history : List[BaseMessage]
            The message history.

        Returns
        -------
        Tuple[str, List[ContentArtifact]]
            The response message and content artifacts.
        """
    
        return await self._arun(
            prompt = user_prompt,
            intents = intents,
            message_history = message_history,
            runnable_config = None)

    def create_tool_content_artifact(
            self,
            original_prompt: str,
            tool_input: str = None,
            prompt_tokens: int = 0,
            completion_tokens : int = 0
    ) -> ContentArtifact:
        """
        Creates a tool content artifact.
        """

        tool_artifact = ContentArtifact(id=self.name)
        tool_artifact.source = self.name
        tool_artifact.type = CONTENT_ARTIFACT_TYPE_TOOL_EXECUTION
        tool_artifact.content = original_prompt
        tool_artifact.title = self.name
        tool_artifact.filepath = None
        tool_artifact.metadata = {
            'tool_input': tool_input,
            'prompt_tokens': str(prompt_tokens),
            'completion_tokens': str(completion_tokens)
        }

        return tool_artifact

    def create_error_content_artifact(
            self,
            original_prompt: str,
            e: Exception
    ) -> ContentArtifact:
        """
        Creates an error content artifact.
        """

        error_artifact = ContentArtifact(id=self.name)
        error_artifact.source = self.name
        error_artifact.type = CONTENT_ARTIFACT_TYPE_TOOL_ERROR
        error_artifact.content = repr(e)
        error_artifact.title = f'{self.name} Error'
        error_artifact.metadata = {
            'tool': self.name,
            'error_message': str(e),
            'prompt': original_prompt
        }
        return error_artifact
    
    def __create_main_llm(self):
        """ Creates the main conversations LLM instance and saves it to self.main_llm. """

        model_object_id = self.tool_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
            AIModelResourceTypeNames.AI_MODELS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_MODEL
        )

        main_llm_model_object_id = model_object_id.object_id
        main_llm_model_properties = self.objects[main_llm_model_object_id]
        self.main_llm_deployment_name = main_llm_model_properties['deployment_name']
        main_llm_endpoint_object_id = main_llm_model_properties['endpoint_object_id']
        main_llm_deployment_name = main_llm_model_properties['deployment_name']
        main_llm_endpoint_properties = self.objects[main_llm_endpoint_object_id]
        main_llm_endpoint_url = main_llm_endpoint_properties['url']
        main_llm_endpoint_api_version = main_llm_endpoint_properties['api_version']
        main_llm_endpoint_api_authentication_type = main_llm_endpoint_properties['authentication_type']
        if main_llm_endpoint_api_authentication_type == AuthenticationTypes.API_KEY:

            main_llm_endpoint_authentication_parameters = main_llm_endpoint_properties['authentication_parameters']
            main_llm_endpoint_api_key = self.config.get_value(
                main_llm_endpoint_authentication_parameters.get('api_key_configuration_name'))

            self.main_llm = AzureChatOpenAI(
                azure_endpoint=main_llm_endpoint_url,
                api_version=main_llm_endpoint_api_version,
                openai_api_type='azure_ad',
                api_key=main_llm_endpoint_api_key,
                azure_deployment=main_llm_deployment_name,
                max_retries=0,
                timeout=30.0
            )
        else:
            scope = 'https://cognitiveservices.azure.com/.default'
            # Set up a Azure AD token provider.
            token_provider = get_bearer_token_provider(
                self.default_credential,
                scope
            )

            self.main_llm = AzureChatOpenAI(
                azure_endpoint=main_llm_endpoint_url,
                api_version=main_llm_endpoint_api_version,
                openai_api_type='azure_ad',
                azure_ad_token_provider=token_provider,
                azure_deployment=main_llm_deployment_name,
                max_retries=0,
                timeout=30.0
            )

    def __create_main_prompt(self):
        
        prompt_object_id = self.tool_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_PROMPT,
            PromptResourceTypeNames.PROMPTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_PROMPT
        )

        if prompt_object_id:
            main_prompt_object_id = prompt_object_id.object_id
            main_prompt_properties = self.objects[main_prompt_object_id]
            main_prompt = main_prompt_properties['prefix']

            self.main_prompt = main_prompt