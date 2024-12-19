from typing import Any, Self, Literal
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.utils import object_utils
from .agent_workflow_base import AgentWorkflowBase

class ExternalAgentWorkflow(AgentWorkflowBase):
    """
    The configuration for an external agent workflow loaded as a plugin.
    """
    type: Literal["external-agent-workflow"] = "external-agent-workflow"
    
   
    @staticmethod
    def from_object(obj: Any) -> Self:

        workflow: ExternalAgentWorkflow = None

        try:
            workflow = ExternalAgentWorkflow(**object_utils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"The External Agent Workflow object provided is invalid. {str(e)}", 400)
        
        if workflow is None:
            raise LangChainException("The External Agent Workflow object provided is invalid.", 400)

        return workflow
