import uuid
from langchain_community.callbacks import get_openai_callback
from langchain_core.messages import HumanMessage, ToolMessage
from langchain_core.prompts import PromptTemplate
from langchain_core.runnables import RunnablePassthrough, RunnableLambda
from langchain_core.output_parsers import StrOutputParser
from langgraph.prebuilt import create_react_agent
from foundationallm.langchain.agents import LangChainAgentBase
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.langchain.retrievers import RetrieverFactory, ContentArtifactRetrievalBase
from foundationallm.langchain.workflows import ExternalWorkflowFactory
from foundationallm.models.agents import AzureOpenAIAssistantsAgentWorkflow, ExternalAgentWorkflow, LangGraphReactAgentWorkflow
from foundationallm.models.constants import (
    AgentCapabilityCategories,
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,
    AIModelResourceTypeNames,
    PromptResourceTypeNames
)
from foundationallm.models.operations import OperationTypes
from foundationallm.models.orchestration import (
    CompletionRequestObjectKeys,
    CompletionResponse,
    OpenAITextMessageContentItem,
    ContentArtifact
)
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.models.agents import (
    AgentConversationHistorySettings,
    KnowledgeManagementAgent,
    KnowledgeManagementCompletionRequest,
    KnowledgeManagementIndexConfiguration
)
from foundationallm.models.attachments import AttachmentProviders
from foundationallm.models.authentication import AuthenticationTypes
from foundationallm.models.language_models import LanguageModelProvider
from foundationallm.models.resource_providers.vectorization import (
    EmbeddingProfileSettingsKeys,
    AzureAISearchIndexingProfile,
    AzureOpenAIEmbeddingProfile
)
from foundationallm.models.services import OpenAIAssistantsAPIRequest
from foundationallm.services import (
    AudioAnalysisService,
    ImageService,
    OpenAIAssistantsApiService
)
from foundationallm.services.gateway_text_embedding import GatewayTextEmbeddingService
from openai.types import CompletionUsage

from foundationallm.langchain.tools import ToolFactory

class LangChainKnowledgeManagementAgent(LangChainAgentBase):
    """
    The LangChain Knowledge Management agent.
    """

    def _get_document_retriever(
        self,
        request: KnowledgeManagementCompletionRequest,
        agent: KnowledgeManagementAgent):
        """
        Get the vector document retriever, if it exists.
        """
        retriever = None

        if agent.vectorization is not None and not agent.inline_context:
            text_embedding_profile = AzureOpenAIEmbeddingProfile.from_object(
                request.objects[agent.vectorization.text_embedding_profile_object_id]
            )

            # text_embedding_profile has the embedding model name in Settings.
            text_embedding_model_name = text_embedding_profile.settings.get(EmbeddingProfileSettingsKeys.MODEL_NAME)

            # objects dictionary has the gateway API endpoint configuration.
            gateway_endpoint_configuration = APIEndpointConfiguration.from_object(
                request.objects[CompletionRequestObjectKeys.GATEWAY_API_ENDPOINT_CONFIGURATION]
            )

            gateway_embedding_service = GatewayTextEmbeddingService(
                instance_id= self.instance_id,
                user_identity=self.user_identity,
                gateway_api_endpoint_configuration=gateway_endpoint_configuration,
                model_name = text_embedding_model_name,
                config=self.config
                )

            # array of objects containing the indexing profile and associated endpoint configuration
            index_configurations = []

            if (agent.vectorization.indexing_profile_object_ids is not None) and (text_embedding_profile is not None):
                for profile_id in agent.vectorization.indexing_profile_object_ids:

                    indexing_profile = AzureAISearchIndexingProfile.from_object(
                            request.objects[profile_id]
                        )
                    # indexing profile has indexing_api_endpoint_configuration_object_id in Settings.
                    indexing_api_endpoint_configuration = APIEndpointConfiguration.from_object(
                        request.objects[indexing_profile.settings.api_endpoint_configuration_object_id]
                    )

                    index_configurations.append(
                        KnowledgeManagementIndexConfiguration(
                            indexing_profile = indexing_profile,
                            api_endpoint_configuration = indexing_api_endpoint_configuration
                        )
                    )

                retriever_factory = RetrieverFactory(
                                index_configurations=index_configurations,
                                gateway_text_embedding_service=gateway_embedding_service,
                                config=self.config)
                retriever = retriever_factory.get_retriever()
        return retriever

    def _get_prompt_template(
        self,
        request: KnowledgeManagementCompletionRequest,
        conversation_history: AgentConversationHistorySettings) -> PromptTemplate:
        """
        Build a prompt template.
        """
        prompt_builder = ''

        # Add the prefix, if it exists.
        if self.prompt.prefix is not None:
            prompt_builder = f'{self.prompt.prefix}\n\n'

        # Add the message history, if it exists.
        if conversation_history is not None and conversation_history.enabled:
            prompt_builder += self._build_conversation_history(
                request.message_history,
                conversation_history.max_history)

        # Insert the context into the template.
        prompt_builder += '{context}'

        # Add the suffix, if it exists.
        if self.prompt.suffix is not None:
            prompt_builder += f'\n\n{self.prompt.suffix}'

        image_attachments = [attachment for attachment in request.attachments if (attachment.provider == AttachmentProviders.FOUNDATIONALLM_ATTACHMENT and attachment.content_type.startswith('image/'))] if request.attachments is not None else []
        if self.has_retriever or len(image_attachments) > 0:
            # Insert the user prompt into the template.
            prompt_builder += "\n\nQuestion: {question}"

        # Create the prompt template.
        return PromptTemplate.from_template(prompt_builder)

    def _validate_conversation_history(self, conversation_history_settings: AgentConversationHistorySettings):
        """
        Validates that the agent contains all required properties.

        Parameters
        ----------
        agent : KnowledgeManagementAgent
            The agent to validate.
        """
        if conversation_history_settings is None:
            raise LangChainException("The ConversationHistory property of the agent cannot be null.", 400)

        if conversation_history_settings.enabled is None:
            raise LangChainException("The Enabled property of the agent's ConversationHistory property cannot be null.", 400)

        if conversation_history_settings.enabled and conversation_history_settings.max_history is None:
            raise LangChainException("The MaxHistory property of the agent's ConversationHistory property cannot be null.", 400)

    def _validate_request(self, request: KnowledgeManagementCompletionRequest):
        """
        Validates that the completion request contains all required properties.

        Parameters
        ----------
        request : KnowledgeManagementCompletionRequest
            The completion request to validate.
        """
        if request.agent is None:
            raise LangChainException("The agent property on the completion request cannot be null.", 400)

        if request.objects is None:
            raise LangChainException("The objects property on the completion request cannot be null.", 400)

        if request.agent.workflow is not None:
            
            ai_model_object_id = request.agent.workflow.get_resource_object_id_properties(
                ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
                AIModelResourceTypeNames.AI_MODELS,
                ResourceObjectIdPropertyNames.OBJECT_ROLE,
                ResourceObjectIdPropertyValues.MAIN_MODEL
            )
            if ai_model_object_id is None:
                raise LangChainException("The agent's workflow AI models requires a main_model.", 400)
            self.ai_model = self._get_ai_model_from_object_id(ai_model_object_id.object_id, request.objects)
            
            prompt_object_id = request.agent.workflow.get_resource_object_id_properties(
                ResourceProviderNames.FOUNDATIONALLM_PROMPT,
                PromptResourceTypeNames.PROMPTS,
                ResourceObjectIdPropertyNames.OBJECT_ROLE,
                ResourceObjectIdPropertyValues.MAIN_PROMPT
            )
            if prompt_object_id is None:
                raise LangChainException("The agent's workflow prompt object dictionary requires a main_prompt.", 400)
            self.prompt = self._get_prompt_from_object_id(prompt_object_id.object_id, request.objects)
        else:
            # Legacy code
            self.ai_model = self._get_ai_model_from_object_id(request.agent.ai_model_object_id, request.objects)
            self.prompt = self._get_prompt_from_object_id(request.agent.prompt_object_id, request.objects)

        if self.ai_model.endpoint_object_id is None or self.ai_model.endpoint_object_id == '':
            raise LangChainException("The AI model object provided in the request's objects dictionary is invalid because it is missing an endpoint_object_id value.", 400)
        if self.ai_model.deployment_name is None or self.ai_model.deployment_name == '':
            raise LangChainException("The AI model object provided in the request's objects dictionary is invalid because it is missing a deployment_name value.", 400)
        if self.ai_model.model_parameters is None:
            raise LangChainException("The AI model object provided in the request's objects dictionary is invalid because the model_parameters value is None.", 400)

        self.api_endpoint = self._get_api_endpoint_from_object_id(self.ai_model.endpoint_object_id, request.objects)
        if self.api_endpoint.provider is None or self.api_endpoint.provider == '':
            raise LangChainException("The API endpoint object provided in the request's objects dictionary is invalid because it is missing a provider value.", 400)

        try:
            LanguageModelProvider(self.api_endpoint.provider)
        except ValueError:
            raise LangChainException(f"The LLM provider {self.api_endpoint.provider} is not supported.", 400)

        if self.api_endpoint.provider == LanguageModelProvider.MICROSOFT:
            # Verify the api_endpoint_configuration includes the api_version property for Azure OpenAI models.
            if self.api_endpoint.api_version is None or self.api_endpoint.api_version == '':
                raise LangChainException("The api_version property of the api_endpoint_configuration object cannot be null or empty.", 400)

        if self.api_endpoint.url is None or self.api_endpoint.url == '':
            raise LangChainException("The API endpoint object provided in the request's objects dictionary is invalid because it is missing a url value.", 400)
        if self.api_endpoint.authentication_type is None or self.api_endpoint.authentication_type == '':
            raise LangChainException("The API endpoint object provided in the request's objects dictionary is invalid because it is missing an authentication_type value.", 400)

        try:
            AuthenticationTypes(self.api_endpoint.authentication_type)
        except ValueError:
            raise LangChainException(f"The authentication_type {self.api_endpoint.authentication_type} is not supported.", 400)


        if self.prompt.prefix is None or self.prompt.prefix == '':
            raise LangChainException("The Prompt object provided in the request's objects dictionary is invalid because it is missing a prefix value.", 400)

        if request.agent.vectorization is not None and not request.agent.inline_context:
            if request.agent.vectorization.text_embedding_profile_object_id is None or request.agent.vectorization.text_embedding_profile_object_id == '':
                raise LangChainException("The TextEmbeddingProfileObjectId property of the agent's Vectorization property cannot be null or empty.", 400)

            if request.objects.get(request.agent.vectorization.text_embedding_profile_object_id) is None:
                raise LangChainException("The TextEmbeddingProfile object provided in the request's objects dictionary is invalid.", 400)

            # Ensure the text embedding profile has model_name in the settings.
            text_embedding_profile = AzureOpenAIEmbeddingProfile.from_object(request.objects[request.agent.vectorization.text_embedding_profile_object_id])
            if text_embedding_profile.settings.get(EmbeddingProfileSettingsKeys.MODEL_NAME) is None or text_embedding_profile.settings.get(EmbeddingProfileSettingsKeys.MODEL_NAME) == '':
                raise LangChainException("The TextEmbeddingProfile object provided in the request's objects dictionary is invalid because it is missing an embedding_model_name value.", 400)

            if request.agent.vectorization.indexing_profile_object_ids is not None and len(request.agent.vectorization.indexing_profile_object_ids) > 0:
                for idx, indexing_profile in enumerate(request.agent.vectorization.indexing_profile_object_ids):
                    if indexing_profile is None or indexing_profile == '':
                        raise LangChainException(f"The indexing profile object id at index {idx} is invalid.", 400)

                    idx_profile = AzureAISearchIndexingProfile.from_object(request.objects[indexing_profile])
                    if idx_profile.settings.api_endpoint_configuration_object_id is None or idx_profile.settings.api_endpoint_configuration_object_id == '':
                        raise LangChainException(f"The indexing profile object provided in the request's objects dictionary is invalid because it is missing an api_endpoint_configuration_object_id value.", 400)

                self.has_indexing_profiles = True

        # if the OpenAI.Assistants capability is present, validate the following required fields:
        #   AssistantId, AssistantThreadId
        if request.agent.workflow is not None and isinstance(request.agent.workflow, AzureOpenAIAssistantsAgentWorkflow):
            if request.agent.workflow.assistant_id is None or request.agent.workflow.assistant_id == '':
                raise LangChainException("The AzureOpenAIAssistantsAgentWorkflow object provided in the request's agent property is invalid because it is missing an assistant_id value.", 400)
        else:
            # Legacy code
            if "OpenAI.Assistants" in request.agent.capabilities:
                required_fields = [CompletionRequestObjectKeys.OPENAI_ASSISTANT_ID, CompletionRequestObjectKeys.OPENAI_THREAD_ID]
                for field in required_fields:
                    if not request.objects.get(field):
                        raise LangChainException(f"The {field} property is required when the OpenAI.Assistants capability is present.", 400)

        self._validate_conversation_history(request.agent.conversation_history_settings)

    async def invoke_async(self, request: KnowledgeManagementCompletionRequest) -> CompletionResponse:
        """
        Executes an async completion request.
        If a vector index exists, it will be queryied with the user prompt.

        Parameters
        ----------
        request : KnowledgeManagementCompletionRequest
            The completion request to execute.

        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the generated summary, the user_prompt,
            generated full prompt with context and token utilization and execution cost details.
        """
        self._validate_request(request)
        llm = self._get_language_model()

        agent = request.agent
        image_analysis_token_usage = CompletionUsage(prompt_tokens=0, completion_tokens=0, total_tokens=0)

        image_analysis_results = None
        # Get image attachments that are images with URL file paths.
        image_attachments = [attachment for attachment in request.attachments if (attachment.provider == AttachmentProviders.FOUNDATIONALLM_ATTACHMENT and attachment.content_type.startswith('image/'))] if request.attachments is not None else []
        if len(image_attachments) > 0:
            image_client = self._get_language_model(override_operation_type=OperationTypes.IMAGE_SERVICES)
            image_svc = ImageService(config=self.config, client=image_client, deployment_name=self.ai_model.deployment_name)
            image_analysis_results, usage = await image_svc.analyze_images_async(image_attachments)
            if usage is not None:
                image_analysis_token_usage.prompt_tokens += usage.prompt_tokens
                image_analysis_token_usage.completion_tokens += usage.completion_tokens
                image_analysis_token_usage.total_tokens += usage.total_tokens

        audio_analysis_results = None
        audio_attachments = [attachment for attachment in request.attachments if (attachment.provider == AttachmentProviders.FOUNDATIONALLM_ATTACHMENT and attachment.content_type.startswith('audio/'))] if request.attachments is not None else []
        if len(audio_attachments) > 0:
            audio_service = AudioAnalysisService(config=self.config)
            audio_analysis_results = audio_service.classify(request, audio_attachments)

        # Start Assistants API implementation
        # Check for Assistants API capability
        if (agent.workflow is not None and isinstance(agent.workflow, AzureOpenAIAssistantsAgentWorkflow))\
            or (AgentCapabilityCategories.OPENAI_ASSISTANTS in agent.capabilities): # or condition for legacy code

            assistant_id = None
            if agent.workflow is not None and isinstance(agent.workflow, AzureOpenAIAssistantsAgentWorkflow):
                assistant_id = agent.workflow.assistant_id
            else:
                # Legacy code
                assistant_id = request.objects[CompletionRequestObjectKeys.OPENAI_ASSISTANT_ID]

            operation_type_override = OperationTypes.ASSISTANTS_API
            # create the service
            assistant_svc = OpenAIAssistantsApiService(
                azure_openai_client=self._get_language_model(override_operation_type=operation_type_override),
                operations_manager=self.operations_manager
            )

            # populate service request object
            assistant_req = OpenAIAssistantsAPIRequest(
                document_id=str(uuid.uuid4()),
                operation_id=request.operation_id,
                instance_id=self.config.get_value("FoundationaLLM:Instance:Id"),
                assistant_id=assistant_id,
                thread_id=request.objects[CompletionRequestObjectKeys.OPENAI_THREAD_ID],
                attachments=[attachment.provider_file_name for attachment in request.attachments if attachment.provider == AttachmentProviders.FOUNDATIONALLM_AZURE_OPENAI],
                user_prompt=request.user_prompt
            )

            # Add user and assistant messages related to image analysis to the Assistants API request.
            if image_analysis_results is not None:
                # Add user message
                await assistant_svc.add_thread_message_async(
                    thread_id = assistant_req.thread_id,
                    role = "user",
                    content = "Analyze any attached images.",
                    attachments = []
                )
                # Add assistant message
                await assistant_svc.add_thread_message_async(
                    thread_id = assistant_req.thread_id,
                    role = "assistant",
                    content = image_svc.format_results(image_analysis_results),
                    attachments = []
                )

            # Add user and assistant messages related to audio classification to the Assistants API request.
            if audio_analysis_results is not None:
                audio_analysis_context = ''
                for key, value in audio_analysis_results.items():
                    filename = key
                    audio_analysis_context += f'File: {filename}\n'
                    audio_analysis_context += 'Predictions:\n'
                    for prediction in value['predictions']:
                        label = prediction['label']
                        audio_analysis_context += f'- {label}' + '\n'
                    audio_analysis_context += '\n'
                # Add user message
                assistant_svc.add_thread_message_async(
                    thread_id = assistant_req.thread_id,
                    role = "user",
                    content = f"Classify the sounds in the audio file.",
                    attachments = []
                )
                # Add assistant message
                assistant_svc.add_thread_message_async(
                    thread_id = assistant_req.thread_id,
                    role = "assistant",
                    content = audio_analysis_context, # TODO: This will change at some point to accommodate multiple predictions.
                    attachments = []
                )

            image_service = None
            if any(tool.name == "DALLEImageGeneration" for tool in request.agent.tools):
                dalle_tool = next((tool for tool in request.agent.tools if tool.name == "DALLEImageGeneration"), None)

                model_object_id = dalle_tool.get_resource_object_id_properties(
                    ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
                    AIModelResourceTypeNames.AI_MODELS,
                    ResourceObjectIdPropertyNames.OBJECT_ROLE,
                    ResourceObjectIdPropertyValues.MAIN_MODEL
                )
                
                image_generation_deployment_model = request.objects[model_object_id.object_id]["deployment_name"]
                api_endpoint_object_id = request.objects[model_object_id.object_id]["endpoint_object_id"]
                image_generation_client = self._get_image_gen_language_model(api_endpoint_object_id=api_endpoint_object_id, objects=request.objects)
                image_service=ImageService(
                    config=self.config,
                    client=image_generation_client,
                    deployment_name=image_generation_deployment_model,
                    image_generator_tool_description=dalle_tool.description)

            # invoke/run the service
            assistant_response = await assistant_svc.run_async(
                assistant_req,
                image_service=image_service
            )

            # Verify the Assistants API response
            if assistant_response is None:
                print("Assistants API response was None.")
                return CompletionResponse(
                    operation_id = request.operation_id,
                    full_prompt = self.prompt.prefix,
                    user_prompt = request.user_prompt,
                    errors = [ "Assistants API response was None." ]
                )

            # create the CompletionResponse object
            return CompletionResponse(
                id = assistant_response.document_id,
                operation_id = request.operation_id,
                full_prompt = self.prompt.prefix,
                content = assistant_response.content,
                analysis_results = assistant_response.analysis_results,
                completion_tokens = assistant_response.completion_tokens + image_analysis_token_usage.completion_tokens,
                prompt_tokens = assistant_response.prompt_tokens + image_analysis_token_usage.prompt_tokens,
                total_tokens = assistant_response.total_tokens + image_analysis_token_usage.total_tokens,
                user_prompt = request.user_prompt,
                errors = assistant_response.errors
            )
        # End Assistants API implementation

        # Start LangGraph ReAct Agent workflow implementation
        if (agent.workflow is not None and isinstance(agent.workflow, LangGraphReactAgentWorkflow)):
            tool_factory = ToolFactory(self.plugin_manager)
            tools = []

            parsed_user_prompt = request.user_prompt

            explicit_tool = next((tool for tool in agent.tools if parsed_user_prompt.startswith(f'[{tool.name}]:')), None)
            if explicit_tool is not None:
                tools.append(tool_factory.get_tool(explicit_tool, request.objects, self.user_identity, self.config))
                parsed_user_prompt = parsed_user_prompt.split(':', 1)[1].strip()
            else:
                # Populate tools list from agent configuration
                for tool in agent.tools:
                    tools.append(tool_factory.get_tool(tool, request.objects, self.user_identity, self.config))

            # Define the graph
            graph = create_react_agent(llm, tools=tools, state_modifier=self.prompt.prefix)
            messages = self._build_conversation_history_message_list(request.message_history, agent.conversation_history_settings.max_history)
            messages.append(HumanMessage(content=parsed_user_prompt))

            response = await graph.ainvoke(
                {'messages': messages}, 
                config={"configurable": {"original_user_prompt": parsed_user_prompt, **({"recursion_limit": agent.workflow.graph_recursion_limit} if agent.workflow.graph_recursion_limit is not None else {})}}
            )
            # TODO: process tool messages with analysis results AIMessage with content='' but has addition_kwargs={'tool_calls';[...]}

            # Get ContentArtifact items from ToolMessages
            content_artifacts = []
            tool_messages = [message for message in response["messages"] if isinstance(message, ToolMessage)]
            for tool_message in tool_messages:
                if tool_message.artifact is not None:
                    # if the tool message artifact is a list, check if it contains a ContentArtifact item
                    if isinstance(tool_message.artifact, list):
                        for item in tool_message.artifact:
                            if isinstance(item, ContentArtifact):
                                content_artifacts.append(item)

            final_message = response["messages"][-1]
            response_content = OpenAITextMessageContentItem(
                value = final_message.content,
                agent_capability_category = AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
            )
            return CompletionResponse(
                        operation_id = request.operation_id,
                        content = [response_content],
                        content_artifacts = content_artifacts,
                        user_prompt = request.user_prompt,
                        full_prompt = self.prompt.prefix,
                        completion_tokens = final_message.usage_metadata["output_tokens"] or 0,
                        prompt_tokens = final_message.usage_metadata["input_tokens"] or 0,
                        total_tokens = final_message.usage_metadata["total_tokens"] or 0,
                        total_cost = 0
                    )
        # End LangGraph ReAct Agent workflow implementation

        # Start External Agent workflow implementation
        if (agent.workflow is not None and isinstance(agent.workflow, ExternalAgentWorkflow)):
            # prepare tools
            tool_factory = ToolFactory(self.plugin_manager)            
            tools = []

            parsed_user_prompt = request.user_prompt

            explicit_tool = next((tool for tool in agent.tools if parsed_user_prompt.startswith(f'[{tool.name}]:')), None)
            if explicit_tool is not None:
                tools.append(tool_factory.get_tool(explicit_tool, request.objects, self.user_identity, self.config))
                parsed_user_prompt = parsed_user_prompt.split(':', 1)[1].strip()
            else:
                # Populate tools list from agent configuration
                for tool in agent.tools:
                    tools.append(tool_factory.get_tool(tool, request.objects, self.user_identity, self.config))

            # create the workflow
            workflow_factory = ExternalWorkflowFactory(self.plugin_manager)
            workflow = workflow_factory.create_workflow(
                agent.workflow,
                request.objects,
                tools,
                self.user_identity,
                self.config)
           
            # Get message history          
            messages = self._build_conversation_history_message_list(request.message_history, agent.conversation_history_settings.max_history)            

            response = await workflow.invoke_async(
                user_prompt=parsed_user_prompt,
                message_history=messages
            )
            return response
        # End External Agent workflow implementation

        # Start LangChain Expression Language (LCEL) implementation

        # Get the vector document retriever, if it exists.
        retriever = self._get_document_retriever(request, agent)
        if retriever is not None:
            self.has_retriever = True

        # Get the prompt template.
        prompt_template = self._get_prompt_template(
            request,
            agent.conversation_history_settings
        )

        if retriever is not None:
            chain_context = { "context": retriever | retriever.format_docs, "question": RunnablePassthrough() }
        elif image_analysis_results is not None or audio_analysis_results is not None:
            external_analysis_context = ''
            if image_analysis_results is not None:
                external_analysis_context += image_svc.format_results(image_analysis_results)
            if audio_analysis_results is not None:
                for key, value in audio_analysis_results.items():
                    filename = key
                    external_analysis_context += f'File: {filename}\n'
                    external_analysis_context += 'Predictions:\n'
                    for prediction in value['predictions']:
                        label = prediction['label']
                        external_analysis_context += f'- {label}' + '\n'
                    external_analysis_context += '\n'

            chain_context = { "context": lambda x: external_analysis_context, "question": RunnablePassthrough() }
        else:
            chain_context = { "context": RunnablePassthrough() }

        # Compose LCEL chain
        chain = (
            chain_context
            | prompt_template
            | RunnableLambda(self._record_full_prompt)
            | llm
        )

        retvalue = None

        if self.api_endpoint.provider == LanguageModelProvider.BEDROCK:
            if self.has_retriever:
                completion = chain.invoke(request.user_prompt)
            else:
                completion = await chain.ainvoke(request.user_prompt)
            response_content = OpenAITextMessageContentItem(
                value = completion.content,
                agent_capability_category = AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
            )
            retvalue = CompletionResponse(
                operation_id = request.operation_id,
                content = [response_content],
                user_prompt = request.user_prompt,
                full_prompt = self.full_prompt.text,
                completion_tokens = completion.usage_metadata["output_tokens"] + image_analysis_token_usage.completion_tokens,
                prompt_tokens = completion.usage_metadata["input_tokens"] + image_analysis_token_usage.prompt_tokens,
                total_tokens = completion.usage_metadata["total_tokens"] + image_analysis_token_usage.total_tokens,
                total_cost = 0
            )
        else:
            # OpenAI compatible models
            with get_openai_callback() as cb:
                # add output parser to openai callback
                chain = chain | StrOutputParser()
                try:
                    if self.has_retriever:
                        completion = chain.invoke(request.user_prompt)
                    else:
                        completion = await chain.ainvoke(request.user_prompt)

                    response_content = OpenAITextMessageContentItem(
                        value = completion,
                        agent_capability_category = AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
                    )
                    retvalue =  CompletionResponse(
                        operation_id = request.operation_id,
                        content = [response_content],
                        user_prompt = request.user_prompt,
                        full_prompt = self.full_prompt.text,
                        completion_tokens = cb.completion_tokens + image_analysis_token_usage.completion_tokens,
                        prompt_tokens = cb.prompt_tokens + image_analysis_token_usage.prompt_tokens,
                        total_tokens = cb.total_tokens + image_analysis_token_usage.total_tokens,
                        total_cost = cb.total_cost
                    )
                except Exception as e:
                    raise LangChainException(f"An unexpected exception occurred when executing the completion request: {str(e)}", 500)

        if isinstance(retriever, ContentArtifactRetrievalBase):
            retvalue.content_artifacts = retriever.get_document_content_artifacts() or []

        return retvalue
        # End LangChain Expression Language (LCEL) implementation
