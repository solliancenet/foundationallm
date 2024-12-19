"""
Class: ToolFactory
Description: Factory class for creating tools based on the AgentTool configuration.
"""
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.langchain.tools import DALLEImageGenerationTool
from foundationallm.models.agents import AgentTool
from foundationallm.plugins import PluginManager

class ToolFactory:
    """
    Factory class for creating tools based on the AgentTool configuration.
    """
    FLLM_PACKAGE_NAME = "FoundationaLLM"
    DALLE_IMAGE_GENERATION_TOOL_NAME = "DALLEImageGeneration"

    def __init__(self, plugin_manager: PluginManager):
        """
        Initializes the tool factory.

        Parameters
        ----------
        plugin_manager : PluginManager
            The plugin manager object used to load external tools.
        """
        self.plugin_manager = plugin_manager

    def get_tool(
        self,
        tool_config: AgentTool,
        objects: dict,
        user_identity: UserIdentity,
        config: Configuration
    ) -> FoundationaLLMToolBase:
        """
        Creates an instance of a tool based on the tool configuration.
        """
        if tool_config.package_name == self.FLLM_PACKAGE_NAME:
            # internal tools
            match tool_config.name:
                case self.DALLE_IMAGE_GENERATION_TOOL_NAME:
                    return DALLEImageGenerationTool(tool_config, objects, user_identity, config)
        else:
            tool_plugin_manager = None

            if tool_config.package_name in self.plugin_manager.external_modules:
                tool_plugin_manager = self.plugin_manager.external_modules[tool_config.package_name].plugin_manager
                return tool_plugin_manager.create_tool(tool_config, objects, user_identity, config)
            else:
                raise LangChainException(f"Package {tool_config.package_name} not found in the list of external modules loaded by the package manager.")

        raise LangChainException(f"Tool {tool_config.name} not found in package {tool_config.package_name}")

