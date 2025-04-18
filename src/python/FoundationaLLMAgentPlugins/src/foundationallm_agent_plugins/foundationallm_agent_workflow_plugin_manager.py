from typing import List

from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import AgentTool, ExternalAgentWorkflow
from foundationallm.langchain.common import FoundationaLLMWorkflowBase
from foundationallm.plugins import WorkflowPluginManagerBase

from foundationallm_agent_plugins.workflows import AzureAIAgentServiceWorkflow
from foundationallm_agent_plugins.workflows import FoundationaLLMFunctionCallingWorkflow

class FoundationaLLMAgentWorkflowPluginManager(WorkflowPluginManagerBase):

    AZURE_AI_AGENT_SERVICE_WORKFLOW_CLASS_NAME = 'AzureAIAgentServiceWorkflow'
    FOUNDATIONALLM_FUNCTION_CALLING_WORKFLOW_CLASS_NAME = 'FoundationaLLMFunctionCallingWorkflow'
    
    def __init__(self):
        super().__init__()

    def create_workflow(self,
                     workflow_config: ExternalAgentWorkflow,
                     objects: dict,
                     tools: List[AgentTool],
                     user_identity: UserIdentity,
                     config: Configuration) -> FoundationaLLMWorkflowBase:
        """
        Creates a workflow instance based on the workflow configuration.

        Parameters
        ----------
        workflow_config : ExternalAgentWorkflow
            The workflow configuration.
        objects : dict
            The exploded objects assigned from the agent.
        tools : List[FoundationaLLMToolBase]
            The tools assigned to the agent.
        user_identity : UserIdentity
            The user identity of the user initiating the request.
        config : Configuration
            The application configuration for FoundationaLLM.

        Returns
        -------
        FoundationaLLMWorkflowBase
            The workflow instance.
        """
        if workflow_config.class_name == FoundationaLLMAgentWorkflowPluginManager.FOUNDATIONALLM_FUNCTION_CALLING_WORKFLOW_CLASS_NAME:
            return FoundationaLLMFunctionCallingWorkflow(workflow_config, objects, tools, user_identity, config)
        elif workflow_config.class_name == FoundationaLLMAgentWorkflowPluginManager.AZURE_AI_AGENT_SERVICE_WORKFLOW_CLASS_NAME:
            return AzureAIAgentServiceWorkflow(workflow_config, objects, tools, user_identity, config)
        raise ValueError(f'Unknown workflow name: {workflow_config.name}')

    def refresh_tools():
        print('Refreshing tools...') 