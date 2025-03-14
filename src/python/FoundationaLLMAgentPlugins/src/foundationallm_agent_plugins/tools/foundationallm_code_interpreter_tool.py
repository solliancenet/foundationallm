from typing import Optional, Tuple, List
from uuid import uuid4
import json

from langchain_azure_dynamic_sessions import SessionsPythonREPLTool
from langchain_core.callbacks import CallbackManagerForToolRun, AsyncCallbackManagerForToolRun
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.models.agents import AgentTool
from foundationallm.models.orchestration import ContentArtifact
from foundationallm_agent_plugins.common.constants import CONTENT_ARTIFACT_TYPE_TOOL_EXECUTION

class FoundationaLLMCodeInterpreterTool(FoundationaLLMToolBase):

    def __init__(self, tool_config: AgentTool, objects: dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMCodeInterpreterTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """
        super().__init__(tool_config, objects, user_identity, config)
        self.repl = SessionsPythonREPLTool(   
            session_id=tool_config.properties['session_id'] if 'session_id' in tool_config.properties else str(uuid4()),
            pool_management_endpoint=tool_config.properties['pool_management_endpoint']
        )
        self.description = tool_config.description or self.repl.description
    
    
    def _run(self,                 
            python_code: str,            
            run_manager: Optional[CallbackManagerForToolRun] = None
            ) -> str:
        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")
    
    async def _arun(self,                 
            python_code: str,            
            run_manager: Optional[AsyncCallbackManagerForToolRun] = None,
            runnable_config: RunnableConfig = None) -> Tuple[str, List[ContentArtifact]]:
        # SessionsPythonREPLTool only supports synchronous execution.
        # Get the original prompt
        original_prompt = python_code
        if runnable_config is not None and 'original_user_prompt' in runnable_config['configurable']:        
            original_prompt = runnable_config['configurable']['original_user_prompt']            
        result = self.repl.invoke(python_code)
        response = json.loads(result)
        content = response.get('result', '') or response.get('stdout', '') or response.get('stderr', '')
        content_artifact = ContentArtifact(
            id = self.name,
            title = self.name,
            type = CONTENT_ARTIFACT_TYPE_TOOL_EXECUTION,
            content = content,
            metadata = {
                'original_user_prompt': original_prompt,
                'tool_input': python_code,
                'tool_output': response.get('stdout', ''),
                'tool_error': response.get('stderr', ''),
                'tool_result': response.get('result', '')
            }
        )
        return content, [content_artifact]

    
    #def upload_code_gen_reference_data(self, data: pd.DataFrame, remote_file_path: str, remote_file_extension: str = 'parquet'):
#
    #    if not self.code_gen_enabled:
    #        return
#
    #    if remote_file_extension == 'csv':
    #        binary_data = BytesIO(data.to_csv(index=False).encode('utf-8'))
    #    elif remote_file_extension == 'parquet':
    #        binary_data = BytesIO(data.to_parquet(index=False))
#
    #    reponse = self.code_interpreter_tool.upload_file(data=binary_data, remote_file_path=f'{remote_file_path}.{remote_file_extension}')
    #    return reponse
    #async def try_generate_python_code_async(self, prompt: str, retries: int = 2) -> str:
    #    """ Tries to generate Python code from a given prompt. """
    #    valid_python = False
    #    is_retry = False
    #    tries = 1
    #    while not valid_python:
    #        if is_retry:
    #            print(f"Code generation tool did not return valid Python code. Trying again to extract code from response: {result.content}")
    #            self.logger.warning(f"Code generation tool did not return valid Python code. Trying again to extract code from the response: {result.content}")
    #            prompt = f"Extract the Python code from the following:\n\n{result.content}"
#
    #        result = await self.code_gen_llm.ainvoke(self.build_messages(prompt))
    #        self.code_gen_code = result.content
#
    #        if "```" not in result.content:
    #            valid_python = True
    #        else:
    #            is_retry = True
    #            tries += 1
    #            if tries > retries:
    #                raise Exception(f"Code generation tool did not return valid Python code after {retries} retries.")
#
    #    return result.content