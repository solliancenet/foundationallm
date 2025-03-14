from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import AgentTool
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.plugins import ToolPluginManagerBase

from foundationallm_agent_plugins.tools import (
    FoundationaLLMNopTool,
    FoundationaLLMCodeInterpreterTool,
    FoundationaLLMKnowledgeSearchTool
)

class FoundationaLLMAgentToolPluginManager(ToolPluginManagerBase):
    
    FOUNDATIONALLM_CODE_INTERPRETER_TOOL_CLASS = 'FoundationaLLMCodeInterpreterTool'
    FOUNDATIONALLM_KNOWLEDGE_SEARCH_TOOL_CLASS = 'FoundationaLLMKnowledgeSearchTool'

    def __init__(self):
        super().__init__()

    def create_tool(self,
        tool_config: AgentTool,
        objects: dict,
        user_identity: UserIdentity,
        config: Configuration) -> FoundationaLLMToolBase:

        match tool_config.class_name:
            case FoundationaLLMAgentToolPluginManager.FOUNDATIONALLM_CODE_INTERPRETER_TOOL_CLASS:
                return FoundationaLLMCodeInterpreterTool(tool_config, objects, user_identity, config)
            case FoundationaLLMAgentToolPluginManager.FOUNDATIONALLM_KNOWLEDGE_SEARCH_TOOL_CLASS:
                return FoundationaLLMKnowledgeSearchTool(tool_config, objects, user_identity, config)
            case _:
                raise ValueError(f'Unknown tool class: {tool_config.class_name}')

    def refresh_tools():
        print('Refreshing tools...') 