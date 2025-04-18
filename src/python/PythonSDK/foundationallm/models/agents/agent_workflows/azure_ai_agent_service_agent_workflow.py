from pydantic import Field
from typing import Any, Optional, Self, Literal
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.utils import object_utils
from .agent_workflow_base import AgentWorkflowBase

class AzureAIAgentServiceAgentWorkflow(AgentWorkflowBase):
    """
    The configuration for an Azure AI Agent Service agent workflow.
    """
    type: Literal["azure-ai-agent-service-workflow"] = "azure-ai-agent-service-workflow"
    agent_id: str = Field(description="The ID of the agent in the Azure AI Agent Service.")
    vector_store_id: Optional[str] = Field(description="The ID of the vector store for the agent.")
    project_connection_string: Optional[str] = Field(description="The connection string for the Azure AI project.")
   
    @staticmethod
    def from_object(obj: Any) -> Self:
        workflow: AzureAIAgentServiceAgentWorkflow = None
        try:
            workflow = AzureAIAgentServiceAgentWorkflow(**object_utils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"The Azure AI Agent Service agent workflow object provided is invalid. {str(e)}", 400)
        
        if workflow is None:
            raise LangChainException("The Azure AI Agent Service agent workflow object provided is invalid.", 400)

        return workflow
