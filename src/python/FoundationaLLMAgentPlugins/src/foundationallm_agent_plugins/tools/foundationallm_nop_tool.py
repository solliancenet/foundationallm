# Platform imports
from typing import Optional

# LangChain imports
from langchain_core.callbacks import AsyncCallbackManagerForToolRun, CallbackManagerForToolRun
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException

# FoundationaLLM imports
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.models.agents import AgentTool

class FoundationaLLMNopTool(FoundationaLLMToolBase):
    def __init__(self, tool_config: AgentTool, objects: dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMNopTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """
        super().__init__(tool_config, objects, user_identity, config)

    def _run(self,
            prompt: str,
            run_manager: Optional[CallbackManagerForToolRun] = None
            ) -> str:
        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")

    async def _arun(self,
        prompt: str = None,
        runnable_config: RunnableConfig = None,
        run_manager: Optional[AsyncCallbackManagerForToolRun] = None
        ) -> str:

        original_prompt = runnable_config['configurable']['original_user_prompt']
        self.logger.info(f'Running NOP tool with prompt: {original_prompt}')
        return "Sometimes there is a lot of value in doing nothing."
