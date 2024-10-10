import uuid
from langchain_community.callbacks import get_openai_callback
from langchain_core.prompts import PromptTemplate
from langchain_core.runnables import RunnablePassthrough, RunnableLambda
from langchain_core.output_parsers import StrOutputParser
from foundationallm.langchain.agents import LangChainAgentBase
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.langchain.retrievers import RetrieverFactory, CitationRetrievalBase
from foundationallm.models.constants import AgentCapabilityCategories
from foundationallm.models.orchestration import (
    CompletionRequestObjectKeys,
    CompletionResponse,
    OpenAITextMessageContentItem,
    OperationTypes
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
    ImageAnalysisService,
    OpenAIAssistantsApiService
)
from foundationallm.services.gateway_text_embedding import GatewayTextEmbeddingService
from openai.types import CompletionUsage

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

        if request.agent.orchestration_settings is None:
            raise LangChainException("The Orchestration_settings property on the agent cannot be null.", 400)

        if request.objects is None:
            raise LangChainException("The objects property on the completion request cannot be null.", 400)

        self.ai_model = self._get_ai_model_from_object_id(request.agent.ai_model_object_id, request.objects)
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

        self.prompt = self._get_prompt_from_object_id(request.agent.prompt_object_id, request.objects)
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

        if "OpenAI.Assistants" in request.agent.capabilities:
            required_fields = ["OpenAI.AssistantId", "OpenAI.AssistantThreadId"]
            for field in required_fields:
                if not request.objects.get(field):
                    raise LangChainException(f"The {field} property is required when the OpenAI.Assistants capability is present.", 400)

        self._validate_conversation_history(request.agent.conversation_history_settings)

    def invoke(self, request: KnowledgeManagementCompletionRequest) -> CompletionResponse:
        """
        Executes a synchronous completion request.
        If a vector index exists, it will be queried with the user prompt.

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

        agent = request.agent
        image_analysis_token_usage = CompletionUsage(prompt_tokens=0, completion_tokens=0, total_tokens=0)

        image_analysis_results = None
        image_attachments = [attachment for attachment in request.attachments if (attachment.provider == AttachmentProviders.FOUNDATIONALLM_ATTACHMENT and attachment.content_type.startswith('image/'))] if request.attachments is not None else []
        if len(image_attachments) > 0:
            image_analysis_client = self._get_language_model(override_operation_type=OperationTypes.IMAGE_ANALYSIS, is_async=False)
            image_analysis_svc = ImageAnalysisService(config=self.config, client=image_analysis_client, deployment_model=self.ai_model.deployment_name)
            image_analysis_results, usage = image_analysis_svc.analyze_images(image_attachments)
            image_analysis_token_usage.prompt_tokens += usage.prompt_tokens
            image_analysis_token_usage.completion_tokens += usage.completion_tokens
            image_analysis_token_usage.total_tokens += usage.total_tokens

        audio_analysis_results = None
        audio_attachments = [attachment for attachment in request.attachments if (attachment.provider == AttachmentProviders.FOUNDATIONALLM_ATTACHMENT and attachment.content_type.startswith('audio/'))] if request.attachments is not None else []
        if len(audio_attachments) > 0:
            audio_service = AudioAnalysisService(config=self.config)
            audio_analysis_results = audio_service.classify(request, audio_attachments)

        # Check for Assistants API capability
        if AgentCapabilityCategories.OPENAI_ASSISTANTS in agent.capabilities:
            operation_type_override = OperationTypes.ASSISTANTS_API
            # create the service
            assistant_svc = OpenAIAssistantsApiService(
                azure_openai_client=self._get_language_model(override_operation_type=operation_type_override, is_async=False),
                operations_manager=self.operations_manager
            )

            # populate service request object
            assistant_req = OpenAIAssistantsAPIRequest(
                document_id=str(uuid.uuid4()),
                operation_id=request.operation_id,
                instance_id=self.config.get_value("FoundationaLLM:Instance:Id"),
                assistant_id=request.objects["OpenAI.AssistantId"],
                thread_id=request.objects["OpenAI.AssistantThreadId"],
                attachments=[attachment.provider_file_name for attachment in request.attachments if attachment.provider == AttachmentProviders.FOUNDATIONALLM_AZURE_OPENAI],
                user_prompt=request.user_prompt
            )

            # Add user and assistant messages related to image analysis to the Assistants API request.
            if image_analysis_results is not None:
                # Add user message
                assistant_svc.add_thread_message(
                    thread_id = assistant_req.thread_id,
                    role = "user",
                    content = "Analyze any attached images.",
                    attachments = []
                )
                # Add assistant message
                assistant_svc.add_thread_message(
                    thread_id = assistant_req.thread_id,
                    role = "assistant",
                    content = image_analysis_svc.format_results(image_analysis_results),
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
                assistant_svc.add_thread_message(
                    thread_id = assistant_req.thread_id,
                    role = "user",
                    content = f"Classify the sounds in the audio file.",
                    attachments = []
                )
                # Add assistant message
                assistant_svc.add_thread_message(
                    thread_id = assistant_req.thread_id,
                    role = "assistant",
                    content = audio_analysis_context, # TODO: This will change at some point to accommodate multiple predictions.
                    attachments = []
                )

            # invoke/run the service
            assistant_response = assistant_svc.run(assistant_req)

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
                user_prompt = request.user_prompt
            )

        with get_openai_callback() as cb:
            try:
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
                        external_analysis_context += image_analysis_svc.format_results(image_analysis_results)
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
                    | self._get_language_model()
                    | StrOutputParser()
                )

                completion = chain.invoke(request.user_prompt)
                response_content = OpenAITextMessageContentItem(
                    value = completion,
                    agent_capability_category = AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
                )

                citations = []
                if isinstance(retriever, CitationRetrievalBase):
                    citations = retriever.get_document_citations()

                return CompletionResponse(
                    operation_id = request.operation_id,
                    content = [response_content],
                    citations = citations,
                    user_prompt = request.user_prompt,
                    full_prompt = self.full_prompt.text,
                    completion_tokens = cb.completion_tokens + image_analysis_token_usage.completion_tokens,
                    prompt_tokens = cb.prompt_tokens + image_analysis_token_usage.prompt_tokens,
                    total_tokens = cb.total_tokens + image_analysis_token_usage.total_tokens,
                    total_cost = cb.total_cost
                )
            except Exception as e:
                raise LangChainException(f"An unexpected exception occurred when executing the completion request: {str(e)}", 500)

    async def ainvoke(self, request: KnowledgeManagementCompletionRequest) -> CompletionResponse:
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

        agent = request.agent
        image_analysis_token_usage = CompletionUsage(prompt_tokens=0, completion_tokens=0, total_tokens=0)

        image_analysis_results = None
        # Get image attachments that are images with URL file paths.
        image_attachments = [attachment for attachment in request.attachments if (attachment.provider == AttachmentProviders.FOUNDATIONALLM_ATTACHMENT and attachment.content_type.startswith('image/'))] if request.attachments is not None else []
        if len(image_attachments) > 0:
            image_analysis_client = self._get_language_model(override_operation_type=OperationTypes.IMAGE_ANALYSIS, is_async=True)
            image_analysis_svc = ImageAnalysisService(config=self.config, client=image_analysis_client, deployment_model=self.ai_model.deployment_name)
            image_analysis_results, usage = await image_analysis_svc.aanalyze_images(image_attachments)
            image_analysis_token_usage.prompt_tokens += usage.prompt_tokens
            image_analysis_token_usage.completion_tokens += usage.completion_tokens
            image_analysis_token_usage.total_tokens += usage.total_tokens

        audio_analysis_results = None
        audio_attachments = [attachment for attachment in request.attachments if (attachment.provider == AttachmentProviders.FOUNDATIONALLM_ATTACHMENT and attachment.content_type.startswith('audio/'))] if request.attachments is not None else []
        if len(audio_attachments) > 0:
            audio_service = AudioAnalysisService(config=self.config)
            audio_analysis_results = audio_service.classify(request, audio_attachments)

        # Check for Assistants API capability
        if AgentCapabilityCategories.OPENAI_ASSISTANTS in agent.capabilities:
            operation_type_override = OperationTypes.ASSISTANTS_API
            # create the service
            assistant_svc = OpenAIAssistantsApiService(
                azure_openai_client=self._get_language_model(override_operation_type=operation_type_override, is_async=True),
                operations_manager=self.operations_manager
            )

            # populate service request object
            assistant_req = OpenAIAssistantsAPIRequest(
                document_id=str(uuid.uuid4()),
                operation_id=request.operation_id,
                instance_id=self.config.get_value("FoundationaLLM:Instance:Id"),
                assistant_id=request.objects["OpenAI.AssistantId"],
                thread_id=request.objects["OpenAI.AssistantThreadId"],
                attachments=[attachment.provider_file_name for attachment in request.attachments if attachment.provider == AttachmentProviders.FOUNDATIONALLM_AZURE_OPENAI],
                user_prompt=request.user_prompt
            )

            # Add user and assistant messages related to image analysis to the Assistants API request.
            if image_analysis_results is not None:
                # Add user message
                await assistant_svc.aadd_thread_message(
                    thread_id = assistant_req.thread_id,
                    role = "user",
                    content = "Analyze any attached images.",
                    attachments = []
                )
                # Add assistant message
                await assistant_svc.aadd_thread_message(
                    thread_id = assistant_req.thread_id,
                    role = "assistant",
                    content = image_analysis_svc.format_results(image_analysis_results),
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
                assistant_svc.aadd_thread_message(
                    thread_id = assistant_req.thread_id,
                    role = "user",
                    content = f"Classify the sounds in the audio file.",
                    attachments = []
                )
                # Add assistant message
                assistant_svc.aadd_thread_message(
                    thread_id = assistant_req.thread_id,
                    role = "assistant",
                    content = audio_analysis_context, # TODO: This will change at some point to accommodate multiple predictions.
                    attachments = []
                )

            # invoke/run the service
            assistant_response = await assistant_svc.arun(assistant_req)

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
                user_prompt = request.user_prompt
            )

        with get_openai_callback() as cb:
            try:
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
                        external_analysis_context += image_analysis_svc.format_results(image_analysis_results)
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
                    | self._get_language_model()
                    | StrOutputParser()
                )

                # ainvoke isn't working if search is involved in the completion request. Need to dive deeper into how to get this working.
                if self.has_retriever:
                    completion = chain.invoke(request.user_prompt)
                else:
                    completion = await chain.ainvoke(request.user_prompt)

                response_content = OpenAITextMessageContentItem(
                    value = completion,
                    agent_capability_category = AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
                )

                citations = []
                if isinstance(retriever, CitationRetrievalBase):
                    citations = retriever.get_document_citations()

                return CompletionResponse(
                    operation_id = request.operation_id,
                    content = [response_content],
                    citations = citations,
                    user_prompt = request.user_prompt,
                    full_prompt = self.full_prompt.text,
                    completion_tokens = cb.completion_tokens + image_analysis_token_usage.completion_tokens,
                    prompt_tokens = cb.prompt_tokens + image_analysis_token_usage.prompt_tokens,
                    total_tokens = cb.total_tokens + image_analysis_token_usage.total_tokens,
                    total_cost = cb.total_cost
                )
            except Exception as e:
                raise LangChainException(f"An unexpected exception occurred when executing the completion request: {str(e)}", 500)
