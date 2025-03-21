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
from foundationallm.models.orchestration import ContentArtifact
from foundationallm.storage import BlobStorageManager
from foundationallm_agent_plugins.common.constants import CONTENT_ARTIFACT_TYPE_TOOL_EXECUTION, CONTENT_ARTIFACT_TYPE_FILE

class FoundationaLLMCodeInterpreterFile(BaseModel):
    """ A file to upload to the code interpreter. """
    path: str = Field(
        description="The full path to the file in the storage container. Example: 'resource-provider/FoundationaLLM.Attachment/abc123.py'"
    )
    original_file_name: str = Field(
        description="The original name of the file as it should appear in the code interpreter. Example: 'test.py'"
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
        - path: The full path to the file in the storage container
        - original_file_name: The name the file should have in the code interpreter
        - local_file_path: The local path once the file is uploaded to the code interpreter. This path is in the format '/mnt/data/original_file_name'. Example: '/mnt/data/test.py'
        Example:
        {
            "python_code": "import pandas as pd\n\ndf = pd.read_csv('/mnt/data/data.csv')",
            "files": [
                {
                    "path": "resource-provider/FoundationaLLM.Attachment/abc123.csv",
                    "original_file_name": "data.csv",
                    "local_file_path": "/mnt/data/data.csv"
                }
            ]
        }
        """
    )

class FoundationaLLMCodeInterpreterTool(FoundationaLLMToolBase):
    """ A tool for executing Python code in a code interpreter. """
    args_schema: Type[BaseModel] = FoundationaLLMCodeInterpreterToolInput
    BLOB_STORAGE_CONTAINER_NAME: ClassVar[str] = "resource-provider"
    USER_STORAGE_CONTAINER_NAME: ClassVar[str] = "user-storage"

    def __init__(self, tool_config: AgentTool, objects: dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMCodeInterpreterTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """
        super().__init__(tool_config, objects, user_identity, config)
        self.repl = SessionsPythonREPLTool(   
            session_id=tool_config.properties['session_id'] if 'session_id' in tool_config.properties else str(uuid4()),
            pool_management_endpoint=tool_config.properties['pool_management_endpoint']
        )
        self.description = tool_config.description or self.repl.description
        # Setup storage client
        authentication_type = config.get_value('FoundationaLLM:ResourceProviders:Attachment:Storage:AuthenticationType')
        if authentication_type == "AzureIdentity":
            self.storage_client = BlobStorageManager(
                container_name=self.BLOB_STORAGE_CONTAINER_NAME,
                account_name=config.get_value('FoundationaLLM:ResourceProviders:Attachment:Storage:AccountName'),
                authentication_type="AzureIdentity"
            )
            self.user_storage_client = BlobStorageManager(
                container_name=self.USER_STORAGE_CONTAINER_NAME,
                account_name=config.get_value('FoundationaLLM:ResourceProviders:Attachment:Storage:AccountName'),
                authentication_type="AzureIdentity"
            )
        else:
            self.storage_client = BlobStorageManager(
                container_name=self.BLOB_STORAGE_CONTAINER_NAME,
                blob_connection_string=config.get_value('FoundationaLLM:ResourceProviders:Attachment:Storage:ConnectionString'),
                authentication_type="ConnectionString"
            )    
            self.user_storage_client = BlobStorageManager(
                container_name=self.USER_STORAGE_CONTAINER_NAME,
                blob_connection_string=config.get_value('FoundationaLLM:ResourceProviders:Attachment:Storage:ConnectionString'),
                authentication_type="ConnectionString"
            )
    
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
        if runnable_config is not None and 'original_user_prompt' in runnable_config['configurable']:        
            original_prompt = runnable_config['configurable']['original_user_prompt']

        content_artifacts = []
        # Upload any files to the code interpreter
        if files:
            for file in files:              
                # if the file path begins with the container name, remove it
                if file.path.startswith(self.BLOB_STORAGE_CONTAINER_NAME):
                    file.path = file.path[len(self.BLOB_STORAGE_CONTAINER_NAME) + 1:]
                # Get the byte stream of the file through the storage client
                file_bytes = self.storage_client.read_file_content(file.path, read_into_stream=True)
                self.repl.upload_file(data=file_bytes, remote_file_path=file.original_file_name)                
                
        result = self.repl.invoke(python_code)
        files_list = self.repl.list_files()
        if files_list:
            # Disregard the files in the code interpreter that were uploaded (based on the original file name)
            generated_files_list = [file for file in files_list if file.filename not in {f.original_file_name for f in (files or [])}]            
            # Download the files from the code interpreter to the user storage container
            for generated_file in generated_files_list:
                file_stream = self.repl.download_file(remote_file_path=generated_file.filename)                
                # Create path including the user UPN
                generated_file_storage_path = f"{self.user_identity.upn.replace('@', '_')}/{self.repl.session_id}/{generated_file.filename}"
                self.user_storage_client.write_file_content(generated_file_storage_path, file_stream)
                content_artifacts.append(ContentArtifact(
                    id = self.name,
                    title = self.name,
                    type = CONTENT_ARTIFACT_TYPE_FILE,
                    filepath = generated_file_storage_path,                    
                    metadata = {}
                ))
        response = json.loads(result)
        content = str(response.get('result', '')) or str(response.get('stdout', '')) or str(response.get('stderr', ''))
        content_artifacts.append(ContentArtifact(
            id = self.name,
            title = self.name,
            type = CONTENT_ARTIFACT_TYPE_TOOL_EXECUTION,
            content = content,
            metadata = {
                'original_user_prompt': original_prompt,
                'tool_input': python_code,
                'tool_output': str(response.get('stdout', '')),
                'tool_error': str(response.get('stderr', '')),
                'tool_result': str(response.get('result', ''))
            }
        ))        
        return content, content_artifacts
