# Platform imports
from typing import Any, Tuple

# LangChain imports
from langchain_azure_dynamic_sessions import SessionsPythonREPLTool
from langchain_core.callbacks import AsyncCallbackManagerForToolRun, CallbackManagerForToolRun
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException

# FoundationaLLM imports
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.models.agents import AgentTool

class FoundationaLLMCodeInterpreterTool(SessionsPythonREPLTool):

    def __init__(self, tool_config: AgentTool, objects: dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMCodeInterpreterTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """
        super().__init__(
            session_id='skunkworks-0001',
            response_format='content_and_artifact',
            pool_management_endpoint=tool_config.properties['pool_management_endpoint']
        )
