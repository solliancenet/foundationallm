from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import AgentTool
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.plugins import ToolPluginManagerBase

from skunkworks_foundationallm.tools import (
    FoundationaLLMNopTool,
    FoundationaLLMCodeInterpreterTool,
    FoundationaLLMDataAnalysisTool
)

class SkunkworksToolPluginManager(ToolPluginManagerBase):

    FOUNDATIONALLM_NOP_TOOL_NAME = 'FoundationaLLMNopTool'
    FOUNDATIONALLM_CODE_INTERPRETER_TOOL_NAME = 'FoundationaLLMCodeInterpreterTool'
    FOUNDATIONALLM_DATA_ANALYSIS_TOOL_NAME = 'FoundationaLLMDataAnalysisTool'

    def __init__(self):
        pass

    def create_tool(self,
        tool_config: AgentTool,
        objects: dict,
        user_identity: UserIdentity,
        config: Configuration) -> FoundationaLLMToolBase:

        match tool_config.name:
            case SkunkworksToolPluginManager.FOUNDATIONALLM_NOP_TOOL_NAME:
                return FoundationaLLMNopTool(tool_config, objects, user_identity, config)
            case SkunkworksToolPluginManager.FOUNDATIONALLM_CODE_INTERPRETER_TOOL_NAME:
                return FoundationaLLMCodeInterpreterTool(tool_config, objects, user_identity, config)
            case SkunkworksToolPluginManager.FOUNDATIONALLM_DATA_ANALYSIS_TOOL_NAME:
                return FoundationaLLMDataAnalysisTool(tool_config, objects, user_identity, config)
            case _:
                raise ValueError(f"Unknown tool name: {tool_config.name}")

    def refresh_tools():
        print('Refreshing tools...')
