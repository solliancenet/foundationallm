"""
Class: FoundationaLLMWorkflowBase
Description: FoundationaLLM base class for tools that uses the agent workflow model for its configuration.
"""
from abc import ABC, abstractmethod
from typing import List, Optional

from azure.identity import DefaultAzureCredential

from langchain_core.messages import BaseMessage

from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import ExternalAgentWorkflow
from foundationallm.models.messages.message_history_item import MessageHistoryItem
from foundationallm.models.orchestration import CompletionResponse
from foundationallm.telemetry import Telemetry

from .foundationallm_tool_base import FoundationaLLMToolBase

class FoundationaLLMWorkflowBase(ABC):
    """
    FoundationaLLM base class for workflows that uses the agent workflow model for its configuration.
    """
    def __init__(self,
                 workflow_config: ExternalAgentWorkflow,
                 objects: dict,
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
        self.workflow_config = workflow_config
        self.objects = objects
        self.tools = tools if tools is not None else []
        self.user_identity = user_identity
        self.config = config
        self.logger = Telemetry.get_logger(self.workflow_config.name)
        self.tracer = Telemetry.get_tracer(self.workflow_config.name)
        self.default_credential = DefaultAzureCredential(exclude_environment_credential=True)

    @abstractmethod
    async def invoke_async(self,
                           operation_id: str,
                           user_prompt: str,
                           user_prompt_rewrite: Optional[str],
                           message_history: List[MessageHistoryItem])-> CompletionResponse:
        """
        Invokes the workflow asynchronously.

        Parameters
        ----------
        operation_id : str
            The unique identifier of the FoundationaLLM operation.
        user_prompt : str
            The user prompt message.
        message_history : List[MessageHistoryItem]
            The message history.
        """
        pass
