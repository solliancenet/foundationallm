from pydantic import BaseModel, Field
from typing import Any, Self, Optional, Dict
from ..resource_object_ids_model_base import ResourceObjectIdsModelBase
from foundationallm.utils import ObjectUtils
from foundationallm.langchain.exceptions import LangChainException

class AgentWorkflowBase(ResourceObjectIdsModelBase):
    """
    The base class used for an agent workflow.
    """
    type: Optional[str] = Field(None, alias="type")
    workflow_host: str = Field(None, alias="workflow_host")
    name: str = Field(None, alias="name")
    package_name: str=Field(None, alias="package_name")

    @staticmethod
    def from_object(obj: Any) -> Self:
        agent_workflow_base: AgentWorkflowBase = None
        try:
            agent_workflow_base = AgentWorkflowBase(**ObjectUtils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"The Agent Workflow base model object provided is invalid. {str(e)}", 400)

        if agent_workflow_base is None:
            raise LangChainException("The Agent Workflow base model object provided is invalid.", 400)

        return agent_workflow_base
