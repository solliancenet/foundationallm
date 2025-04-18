"""
Class: AzureAIAgentServiceWorkflow
Description: Workflow that integrates with the Azure AI Agent Service.
"""
from azure.identity import DefaultAzureCredential
from azure.ai.projects import AIProjectClient
from azure.ai.projects.models import MessageRole, RunStatus
from typing import Dict, List, Optional
from logging import Logger
from opentelemetry.trace import Tracer
from opentelemetry.trace import SpanKind

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMWorkflowBase,
    FoundationaLLMToolBase
)
from foundationallm.models.agents import ExternalAgentWorkflow
from foundationallm.models.constants import (
    AgentCapabilityCategories,
    AzureAIResourceTypeNames,
    ContentArtifactTypeNames,
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,   
    RunnableConfigKeys
)
from foundationallm.models.messages import MessageHistoryItem
from foundationallm.models.orchestration import (
    CompletionRequestObjectKeys,
    CompletionResponse,
    ContentArtifact,
    FileHistoryItem,
    OpenAITextMessageContentItem,
    OpenAIImageFileMessageContentItem,
    OpenAIFilePathMessageContentItem
)
from foundationallm.telemetry import Telemetry

class AzureAIAgentServiceWorkflow(FoundationaLLMWorkflowBase):
    """
    FoundationaLLM workflow implementing an integration with the Azure AI Agent Service.
    """
    def __init__(self,
                 workflow_config: ExternalAgentWorkflow,
                 objects: Dict,
                 tools: List[FoundationaLLMToolBase],
                 user_identity: UserIdentity,
                 config: Configuration):
        """
        Initializes the FoundationaLLMWorkflowBase class with the workflow configuration.

        Parameters
        ----------
        workflow_config : ExternalAgentWorkflow
            The workflow assigned to the agent.
        objects : dict
            The exploded objects assigned from the agent.
        tools : List[FoundationaLLMToolBase]
            The tools assigned to the agent.
        user_identity : UserIdentity
            The user identity of the user initiating the request.
        config : Configuration
            The application configuration for FoundationaLLM.
        """
        super().__init__(workflow_config, objects, tools, user_identity, config)
        self.name = workflow_config.name
        self.logger : Logger = Telemetry.get_logger(self.name)
        self.tracer : Tracer = Telemetry.get_tracer(self.name)
        self.default_error_message = workflow_config.properties.get(
            'default_error_message',
            'An error occurred while processing the request.') \
            if workflow_config.properties else 'An error occurred while processing the request.'
        self.__resolve_ai_agent_service_resources()
       
    async def invoke_async(self,
                           operation_id: str,
                           user_prompt:str,
                           user_prompt_rewrite: Optional[str],
                           message_history: List[MessageHistoryItem],
                           file_history: List[FileHistoryItem])-> CompletionResponse:
        """
        Invokes the workflow asynchronously.

        Parameters
        ----------
        operation_id : str
            The unique identifier of the FoundationaLLM operation.
        user_prompt : str
            The user prompt message.
        user_prompt_rewrite : str
            The user prompt rewrite message containing additional context to clarify the user's intent.
        message_history : List[BaseMessage]
            The message history.
        file_history : List[FileHistoryItem]
            The file history.
        """
        llm_prompt = user_prompt_rewrite or user_prompt
        with self.client:
            # Add current message to the thread.
            message = self.client.agents.create_message(
                thread_id = self.thread_id,
                role = MessageRole.USER,
                content = llm_prompt
            )
            # Get the response from the agent.
            run = self.client.agents.create_and_process_run(thread_id=self.thread_id, agent_id=self.agent_id)
            if run.status == RunStatus.FAILED:
                raise Exception(f"Azure AI Agent Service run failed: {run.failure_reason}")
                      
            # Get messages from the thread
            messages = self.client.agents.list_messages(thread_id=self.thread_id)
            print(f"Messages: {messages}")

            # Get the last message from the sender
            last_msg = messages.get_last_text_message_by_role("assistant")
            if last_msg:
                print(f"Last Message: {last_msg.text.value}")

        retvalue = CompletionResponse(
            operation_id=operation_id,
            content = "Under development",
            content_artifacts=[],
            user_prompt=llm_prompt,
            full_prompt="Under development",
            completion_tokens=0,
            prompt_tokens=0,
            total_tokens=0,
            total_cost=0
        )
        return retvalue    
    
    def __resolve_ai_agent_service_resources(self):
        """
        Resolves the Azure AI Agent Service resources from the objects dictionary.
        Populates the agent_id, thread_id, and creates the client based on the Azure AI Project.
        """
        # Populate the agent id and thread id from the objects dictionary
        self.agent_id = self.objects.get(CompletionRequestObjectKeys.AZUREAI_AGENT_ID)
        self.thread_id = self.objects.get(CompletionRequestObjectKeys.AZUREAI_AGENT_THREAD_ID)
        if not self.agent_id:
            raise ValueError('Azure AI Agent Service Agent ID is not set in the objects dictionary.')    
        if not self.thread_id:
            raise ValueError('Azure AI Agent Service Thread ID is not set in the objects dictionary.')
        
        project_object_id = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_AZUREAI,
            AzureAIResourceTypeNames.PROJECTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.AI_PROJECT            
        )
        if not project_object_id:
            raise ValueError('Azure AI Project ID is not set.')
        
        # Get the Project object from the objects dictionary keyed by project_object_id, object[project_object_id]
        project_object = self.objects.get(project_object_id.object_id)
        if not project_object:
            raise ValueError('Azure AI Project object not found in objects dictionary.')
                
        # Get the Azure AI Agent Service client only supports Azure Identity.
        credential = DefaultAzureCredential(exclude_environment_credential=True)
        self.client = AIProjectClient.from_connection_string(
            credential = credential,
            conn_str = project_object['project_connection_string']
        )

    def __create_workflow_execution_content_artifact(
            self,
            original_prompt: str,            
            prompt_tokens: int = 0,
            completion_tokens: int = 0,
            completion_time_seconds: float = 0) -> ContentArtifact:

        content_artifact = ContentArtifact(id=self.workflow_config.name)
        content_artifact.source = self.workflow_config.name
        content_artifact.type = ContentArtifactTypeNames.WORKFLOW_EXECUTION
        content_artifact.content = original_prompt
        content_artifact.title = self.workflow_config.name
        content_artifact.filepath = None
        content_artifact.metadata = {            
            'prompt_tokens': str(prompt_tokens),
            'completion_tokens': str(completion_tokens),
            'completion_time_seconds': str(completion_time_seconds)
        }
        return content_artifact
