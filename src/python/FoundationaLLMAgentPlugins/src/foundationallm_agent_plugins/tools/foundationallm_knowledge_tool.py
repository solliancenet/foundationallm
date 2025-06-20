# pylint: disable=W0221

from typing import List, Optional, Tuple, Type

from pydantic import BaseModel

from langchain_core.callbacks import CallbackManagerForToolRun, AsyncCallbackManagerForToolRun
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMToolBase,
    FoundationaLLMToolResult
)
from foundationallm.models.agents import AgentTool, VectorDatabaseConfiguration
from foundationallm.models.constants import (
    ContentArtifactTypeNames,
    ResourceObjectIdPropertyNames,
    RunnableConfigKeys
)
from foundationallm.models.orchestration import CompletionRequestObjectKeys, ContentArtifact
from foundationallm.models.resource_providers import ResourcePath
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.services.gateway_text_embedding import GatewayTextEmbeddingService
from foundationallm.utils import ObjectUtils

from foundationallm_agent_plugins.utils import AzureAISearchConversationRetriever

from .foundationallm_knowledge_tool_input import FoundationaLLMKnowledgeToolInput

class FoundationaLLMKnowledgeTool(FoundationaLLMToolBase):
    """
    FoundationaLLM knowledge tool.
    """
    args_schema: Type[BaseModel] = FoundationaLLMKnowledgeToolInput

    def __init__(self, tool_config: AgentTool, objects: dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMKnowledgeTool class with the tool configuration,
            exploded objects collection, and platform configuration. """
        super().__init__(tool_config, objects, user_identity, config)
        self.main_llm = self.get_main_language_model()
        self.main_prompt = self.get_main_prompt()
        self.vector_database = self._get_vector_database()
        self.vector_database_api_endpoint_configuration = ObjectUtils.get_object_by_id(
                self.vector_database['api_endpoint_configuration_object_id'],
                self.objects,
                APIEndpointConfiguration)
        self.embedding_service = self._get_embedding_service()

        self.vector_database_configuration = VectorDatabaseConfiguration(
            vector_database=self.vector_database,
            vector_database_api_endpoint_configuration=self.vector_database_api_endpoint_configuration
        )

        # When configuring the tool on an agent, the description will be set providing context to the document source.
        self.description = self.tool_config.description or "Answers questions by searching through documents."

        self.vector_store_names = self.tool_config.properties.get('vector_stores', [])

    def _run(self,
            prompt: str,
            file_name: Optional[str] = None,
            run_manager: Optional[CallbackManagerForToolRun] = None
            ) -> str:
        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")

    async def _arun(self,
            prompt: str,
            file_name: Optional[str] = None,
            run_manager: Optional[AsyncCallbackManagerForToolRun] = None,
            runnable_config: RunnableConfig = None,
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """ Retrieves documents from an index based on the proximity to the prompt to answer the prompt."""

        input_tokens = 0
        output_tokens = 0

        # Azure AI Search retriever only supports synchronous execution.
        # Get the original prompt
        if runnable_config is None:
            raise ToolException("RunnableConfig is required for the execution of the tool.")

        # Finalize the list of vector store names to be used.
        if RunnableConfigKeys.CONVERSATION_ID in runnable_config['configurable']:
            conversation_id = runnable_config['configurable'][RunnableConfigKeys.CONVERSATION_ID]
            current_vector_store_names = [conversation_id if x == '__CONVERSATION__' else x for x in self.vector_store_names]
        else:
            current_vector_store_names = [x for x in self.vector_store_names if x != '__CONVERSATION__']

        if len(current_vector_store_names) == 0:
            raise ToolException("No vector store names provided in the tool configuration.")

        user_prompt = runnable_config['configurable'][RunnableConfigKeys.ORIGINAL_USER_PROMPT] \
            if RunnableConfigKeys.ORIGINAL_USER_PROMPT in runnable_config['configurable'] \
            else None
        user_prompt_rewrite = runnable_config['configurable'][RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE] \
            if RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE in runnable_config['configurable'] \
            else None
        original_prompt = user_prompt_rewrite or user_prompt or prompt

        retriever = self._get_document_retriever(
            vector_store_name=current_vector_store_names[0],
            file_name=file_name)
        docs = retriever.invoke(prompt)
        context = retriever.format_documents(docs)
        completion_prompt = self.main_prompt.replace('{{context}}', context).replace('{{prompt}}', prompt)

        completion = await self.main_llm.ainvoke(completion_prompt)
        input_tokens += completion.usage_metadata['input_tokens']
        output_tokens += completion.usage_metadata['output_tokens']
        content_artifacts = [] # self.retriever.get_document_content_artifacts() or []
        # Token usage content artifact
        # Transform all completion.usage_metadata property values to string
        metadata = {
            'prompt_tokens': str(input_tokens),
            'completion_tokens': str(output_tokens),
            'input_prompt': prompt,
            'input_file_name': file_name,
            'vector_store_names': ', '.join(current_vector_store_names)
        }
        content_artifacts.append(ContentArtifact(
            id = self.name,
            title = self.name,
            content = original_prompt,
            source = self.name,
            type = ContentArtifactTypeNames.TOOL_EXECUTION,
            metadata=metadata))

        return completion.content, FoundationaLLMToolResult(
            content=completion.content,
            content_artifacts=content_artifacts,
            input_tokens=input_tokens,
            output_tokens=output_tokens
        )

    def _get_vector_database(self) -> dict:
        """
        Retrieves a vector database based on the resource object identifier with a specified role.
        """

        vector_database_object_id = self.tool_config.get_resource_object_id_properties(
            "FoundationaLLM.Vector",
            "vectorDatabases",
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            "vector_database"
        )

        if vector_database_object_id:
            vector_database_properties = self.objects[vector_database_object_id.object_id]

            return {
                "database_type": vector_database_properties["database_type"],
                "database_name": vector_database_properties["database_name"],
                "embedding_property_name": vector_database_properties["embedding_property_name"],
                "content_property_name": vector_database_properties["content_property_name"],
                "metadata_property_name": vector_database_properties["metadata_property_name"],
                "vector_store_id_property_name": vector_database_properties["vector_store_id_property_name"],
                "api_endpoint_configuration_object_id": vector_database_properties["api_endpoint_configuration_object_id"],
                "similarity_threshold": self.tool_config.properties.get('similarity_threshold', 0.85),
                "top_n": self.tool_config.properties.get('top_n', 10)
            }

        self.logger.warning("No vector database object identifier found for the specified role.")
        return None

    def _get_embedding_service(self) -> GatewayTextEmbeddingService:

        # Only supporting GatewayTextEmbedding
        # Objects dictionary has the gateway API endpoint configuration by default.
        gateway_endpoint_configuration = ObjectUtils.get_object_by_id(
            CompletionRequestObjectKeys.GATEWAY_API_ENDPOINT_CONFIGURATION,
            self.objects,
            APIEndpointConfiguration)

        gateway_embedding_service = GatewayTextEmbeddingService(
            instance_id= ResourcePath.parse(gateway_endpoint_configuration.object_id).instance_id,
            user_identity=self.user_identity,
            gateway_api_endpoint_configuration=gateway_endpoint_configuration,
            model_name = self.tool_config.properties['embedding_model'],
            config=self.config,
            model_dimensions=int(self.tool_config.properties['embedding_dimensions']))

        return gateway_embedding_service

    def _get_document_retriever(
            self,
            vector_store_name: str,
            file_name:Optional[str] = None) -> AzureAISearchConversationRetriever:
        """
        Gets the document retriever
        """
        retriever = AzureAISearchConversationRetriever(
            vector_store_name=vector_store_name,
            file_name=file_name,
            vector_database_configuration = self.vector_database_configuration,
            gateway_text_embedding_service=self.embedding_service
        )

        return retriever

