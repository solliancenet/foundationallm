from typing import Optional, Tuple, Type, List, ClassVar
from uuid import uuid4
import json

from langchain_azure_dynamic_sessions import SessionsPythonREPLTool
from langchain_core.callbacks import CallbackManagerForToolRun, AsyncCallbackManagerForToolRun
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException

from pydantic import BaseModel, Field

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.models.agents import AgentTool
from foundationallm.models.constants import (
    ContentArtifactTypeNames,
    RunnableConfigKeys
)
from foundationallm.models.orchestration import CompletionRequestObjectKeys, ContentArtifact
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.services import HttpClientService
class FoundationaLLMCodeInterpreterFile(BaseModel):
    """ A file to upload to the code interpreter. """
    file_name: str = Field(
        description="The name of the file as it should appear in the code interpreter. Example: 'test.py'"
    )
    local_file_path: str = Field(
        description="The local path once the file is uploaded to the code interpreter. This path is in the format '/mnt/data/original_file_name'. Example: '/mnt/data/test.py'"
    )

class FoundationaLLMCodeInterpreterToolInput(BaseModel):
    """ Input data model for the Code Intepreter tool. """
    python_code: str = Field(
        description="The Python code to execute. This should be the complete code including any file operations and required pip installations."
    )
    files: Optional[List[FoundationaLLMCodeInterpreterFile]] = Field(
        default=None,
        description="""List of files to upload to the code interpreter. Each file should have:
        - file_name: The name the file should have in the code interpreter
        - local_file_path: The local path once the file is uploaded to the code interpreter. This path is in the format '/mnt/data/original_file_name'. Example: '/mnt/data/test.py'
        Example:
        {
            "python_code": "import pandas as pd\n\ndf = pd.read_csv('/mnt/data/data.csv')",
            "files": [
                {
                    "file_name": "data.csv",
                    "local_file_path": "/mnt/data/data.csv"
                }
            ]
        }
        """
    )

class FoundationaLLMCodeInterpreterTool(FoundationaLLMToolBase):
    """ A tool for executing Python code in a code interpreter. """
    args_schema: Type[BaseModel] = FoundationaLLMCodeInterpreterToolInput
    DYNAMIC_SESSION_ENDPOINT: ClassVar[str] = "code_session_endpoint"
    DYNAMIC_SESSION_ID: ClassVar[str] = "code_session_id"

    def __init__(self, tool_config: AgentTool, objects: dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMCodeInterpreterTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """
        super().__init__(tool_config, objects, user_identity, config)
        self.repl = SessionsPythonREPLTool(
            session_id=objects[tool_config.name][self.DYNAMIC_SESSION_ID],
            pool_management_endpoint=objects[tool_config.name][self.DYNAMIC_SESSION_ENDPOINT]
        )
        self.description = tool_config.description or self.repl.description
        context_api_endpoint_configuration = APIEndpointConfiguration(**objects.get(CompletionRequestObjectKeys.CONTEXT_API_ENDPOINT_CONFIGURATION, None))
        if context_api_endpoint_configuration:
            self.context_api_client = HttpClientService(
                context_api_endpoint_configuration,
                user_identity,
                config
            )
        else:
            raise ToolException("The Context API endpoint configuration is required to use the Code Interpreter tool.")
        self.instance_id = objects.get(CompletionRequestObjectKeys.INSTANCE_ID, None)

    def _run(self,
            python_code: str,
            files: Optional[List[FoundationaLLMCodeInterpreterFile]],
            run_manager: Optional[CallbackManagerForToolRun] = None
            ) -> str:
        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")

    async def _arun(self,
            python_code: str,
            files: Optional[List[FoundationaLLMCodeInterpreterFile]] = None,
            run_manager: Optional[AsyncCallbackManagerForToolRun] = None,
            runnable_config: RunnableConfig = None) -> Tuple[str, List[ContentArtifact]]:
        # SessionsPythonREPLTool only supports synchronous execution.
        # Get the original prompt
        original_prompt = python_code
        if runnable_config is not None and RunnableConfigKeys.ORIGINAL_USER_PROMPT in runnable_config['configurable']:
            original_prompt = runnable_config['configurable'][RunnableConfigKeys.ORIGINAL_USER_PROMPT]

        content_artifacts = []
        operation_id = None

        # Upload any files required by this execution to the code interpreter
        file_names = [f.file_name for f in files] if files else []

        # returns the operation_id
        self.context_api_client.headers['X-USER-IDENTITY'] = self.user_identity.model_dump_json()
        operation_response = await self.context_api_client.post_async(
            endpoint = f"/instances/{self.instance_id}/codeSessions/{self.repl.session_id}/uploadFiles",
            data = json.dumps({
                "file_names": file_names
            })
        )
        operation_id = operation_response['operation_id']

        # Obtain beginning file list from the context API
        beginning_files_list = []
        beginning_files_list_response = await self.context_api_client.post_async(
                endpoint = f"/instances/{self.instance_id}/codeSessions/{self.repl.session_id}/downloadFiles",
                data = json.dumps({
                    "operation_id": operation_id
                })
            )
        beginning_files_list = beginning_files_list_response['file_records']

        # Execute the code
        result = self.repl.invoke(python_code)

        # Get an updated list of files from the code interpreter
        files_list = []
        if operation_id:
            files_list_response = await self.context_api_client.post_async(
                endpoint = f"/instances/{self.instance_id}/codeSessions/{self.repl.session_id}/downloadFiles",
                data = json.dumps({
                    "operation_id": operation_id
                })
            )
            files_list = files_list_response['file_records']
            # Remove files that were already present in the beginning of the session
            files_list = {key: value for key, value in files_list.items() if key not in beginning_files_list}

        if files_list:
            # Download the files from the code interpreter to the user storage container
            for file_name, file_data in files_list.items():
                content_artifacts.append(ContentArtifact(
                    id = self.name,
                    title = self.name,
                    type = ContentArtifactTypeNames.FILE,
                    filepath = file_name,
                    metadata = {
                        'file_object_id': file_data['file_object_id'],
                        'original_file_name': file_data['file_name'],
                        'file_path': file_data['file_path'],
                        'file_size': str(file_data['file_size_bytes']),
                        'content_type': file_data['content_type'],
                        'conversation_id': file_data['conversation_id']
                    }
                ))

        response = json.loads(result)
        content = str(response.get('result', '')) or str(response.get('stdout', '')) or str(response.get('stderr', ''))
        content_artifacts.append(ContentArtifact(
            id = self.name,
            title = self.name,
            type = ContentArtifactTypeNames.TOOL_EXECUTION,
            filepath = str(uuid4()), # needs to have a unique filepath to not be filtered out upstream.
            metadata = {
                'original_user_prompt': original_prompt,
                'tool_input_python_code': python_code,
                'tool_input_files': ', '.join([f.file_name for f in files]) if files else '',
                'tool_output': str(response.get('stdout', '')),
                'tool_error': str(response.get('stderr', '')),
                'tool_result': str(response.get('result', ''))
            }
        ))
        return content, content_artifacts
