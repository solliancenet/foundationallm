"""
Class: OpenAIAssistantsApiService
Description: Integration with the OpenAI Assistants API.
"""
import os
from typing import List, Union
from openai import AsyncAzureOpenAI, AzureOpenAI
from openai.pagination import AsyncCursorPage, SyncCursorPage
from openai.types import FileObject
from openai.types.beta.threads import (
    FileCitationAnnotation,
    FilePathAnnotation,
    ImageFileContentBlock,
    ImageURLContentBlock,
    Message,
    TextContentBlock
)
from openai.types.beta.threads.message import Attachment
from openai.types.beta.threads.runs import (
    CodeInterpreterToolCall,
    RunStep
)
from foundationallm.models.constants import AgentCapabilityCategories
from foundationallm.models.orchestration import (
    OpenAIFilePathMessageContentItem,
    OpenAIImageFileMessageContentItem,
    OpenAITextMessageContentItem,
    AnalysisResult
)
from foundationallm.models.services import OpenAIAssistantsAPIRequest, OpenAIAssistantsAPIResponse
from foundationallm.models.attachments import AttachmentProperties
from foundationallm.config import Configuration

class OpenAIAssistantsApiService:
    """
    Integration with the OpenAI Assistants API.
    """

    def __init__(self, azure_openai_client: Union[AzureOpenAI, AsyncAzureOpenAI], config : Configuration):
        """
        Initializes an OpenAI Assistants API service.

        Parameters
        ----------
        azure_openai_client : AzureOpenAI
            Azure OpenAI client for interacting with the OpenAI Assistants API.
            TODO: AzureOpenAI extends OpenAI, test with OpenAI client as input at some point, for now just focus on Azure.
        """
        self.client = azure_openai_client

        #split string and trim whitespace
        self.file_tool_file_types = [x.strip() for x in config.get_value('FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AzureOpenAIAssistantsFileSearchFileExtensions').split(",")] #["c", "cpp", "cs", "css", "doc", "docx", "html", "java", "js", "json", "md", "pdf", "php", "pptx", "py", "rb", "sh", "tex", "ts", "txt"]
        self.code_tool_file_types = [x.strip() for x in config.get_value('FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AzureOpenAIAssistantsCodeInterpreterFileExtensions').split(",")] #["c", "cpp", "cs", "css", "doc", "docx", "html", "java", "js", "json", "md", "pdf", "php", "pptx", "py", "rb", "sh", "tex", "ts", "txt"]

        self.tools = [
            {
                "type": "file_search"
            },
            {
                "type": "code_interpreter"
            }
        ]

    def create_file_attachment(self, attachment : AttachmentProperties):
        file_attachment = {}
        file_attachment['file_id'] = attachment.provider_file_name
        file_attachment['tools'] = []

        ext = os.path.splitext(attachment.original_file_name)[1].replace('.','')

        for tool in self.tools:
            if tool['type'] == 'file_search':
                if ext in self.file_tool_file_types:
                    file_attachment['tools'].append(tool)

            if tool['type'] == 'code_interpreter':
                if ext in self.code_tool_file_types:
                    file_attachment['tools'].append(tool)

        return file_attachment

    async def aadd_thread_message(self, thread_id: str, role: str, content: str, attachments: list = None):
        return await self.client.beta.threads.messages.create(
            thread_id = thread_id,
            role = role,
            content = content,
            attachments = attachments
        )

    def add_thread_message(self, thread_id: str, role: str, content: str, attachments: list = None):
        return self.client.beta.threads.messages.create(
            thread_id = thread_id,
            role = role,
            content = content,
            attachments = attachments
        )

    def run(self, request: OpenAIAssistantsAPIRequest) -> OpenAIAssistantsAPIResponse:
        """
        Creates an OpenAI Assistant Run and executes it.

        Parameters
        ----------
        request : OpenAIAssistantsAPIRequest
            The request to run with the OpenAI Assistants API service.
        """

        # Process file attachments and assign tools
        attachments = self._get_request_attachments(request)

        # Add User prompt to the thread
        message = self.add_thread_message(
            thread_id = request.thread_id,
            role = "user",
            content = request.user_prompt,
            attachments = attachments
        )

        # Create and execute the run
        run = self.client.beta.threads.runs.create_and_poll(
            thread_id = request.thread_id,
            assistant_id = request.assistant_id
        )

        # Retrieve the messages in the thread after the prompt message was appended.
        messages = self.client.beta.threads.messages.list(
            thread_id = request.thread_id, order="asc", after=message.id
        )

        # Retrieve the steps from the run_steps for the analysis
        run_steps = self.client.beta.threads.runs.steps.list(
          thread_id = request.thread_id,
          run_id = run.id
        )

        analysis_results = self._parse_run_steps(run_steps.data)

        content = self._parse_messages(messages)

        return OpenAIAssistantsAPIResponse(
            content = content,
            analysis_results = analysis_results,
            completion_tokens = run.usage.completion_tokens,
            prompt_tokens = run.usage.prompt_tokens,
            total_tokens = run.usage.total_tokens
        )

    async def arun(self, request: OpenAIAssistantsAPIRequest) -> OpenAIAssistantsAPIResponse:
        """
        Creates an OpenAI Assistant Run and executes it asynchronously.

        Parameters
        ----------
        request : OpenAIAssistantsAPIRequest
            The request to run with the OpenAI Assistants API service.

        Returns
        -------
        OpenAIAssistantsAPIResponse
            The response parsed from the OpenAI Assistants API service response.
        """

        # Process file attachments and assign tools
        attachments = await self._aget_request_attachments(request)

        # Add User prompt to the thread
        message = await self.aadd_thread_message(
            thread_id = request.thread_id,
            role = "user",
            content = request.user_prompt,
            attachments = attachments
        )

        # Create and execute the run
        run = await self.client.beta.threads.runs.create_and_poll(
            thread_id = request.thread_id,
            assistant_id = request.assistant_id
            )

        # Retrieve the steps from the run_steps for the analysis
        run_steps = await self.client.beta.threads.runs.steps.list(
          thread_id = request.thread_id,
          run_id = run.id
        )

        analysis_results = await self._aparse_run_steps(run_steps.data)

        # Retrieve the messages in the thread after the prompt message was appended.
        messages = await self.client.beta.threads.messages.list(
            thread_id=request.thread_id, order="asc", after=message.id
        )

        content = await self._aparse_messages(messages)

        return OpenAIAssistantsAPIResponse(
            content = content,
            analysis_results = analysis_results,
            completion_tokens = run.usage.completion_tokens,
            prompt_tokens = run.usage.prompt_tokens,
            total_tokens = run.usage.total_tokens
        )

    def _create_attachment_from_fileobject(self, file:FileObject):
        """
        Creates an attachment from a file object.

        Parameters
        ----------
        file : FileObject
            The file object to create an attachment from.

        Returns
        -------
        Attachment
            The attachment created from the file object.
        """
        #Get the filename extension if it exists
        filename_extension = file.filename.split('.')[-1] if '.' in file.filename else None
        file_search_supported_extensions = ["c", "cpp", "cs", "css", "doc", "docx", "html", "java", "js", "json", "md", "pdf", "php", "pptx", "py", "rb", "sh", "tex", "ts", "txt"]
        tools = [{"type": "code_interpreter"}]
        if filename_extension in file_search_supported_extensions:
            tools.append({"type": "file_search"})
        return Attachment(
            file_id=file.id,
            tools = tools
        )

    def _get_request_attachments(self, request: OpenAIAssistantsAPIRequest):
        """
        Retrieves the attachments from the request.

        Parameters
        ----------
        request : OpenAIAssistantsAPIRequest
            The request to retrieve attachments from.

        Returns
        -------
        List[Attachment]
            The attachments retrieved from the request.
        """
        attachments = []
        if request.attachments:
            for file_id in request.attachments:
                oai_file = self.client.files.retrieve(file_id)
                attachments.append(
                     self._create_attachment_from_fileobject(oai_file)
                  )
        return attachments

    async def _aget_request_attachments(self, request: OpenAIAssistantsAPIRequest):
        """
        Retrieves the attachments from the request asynchronously.

        Parameters
        ----------
        request : OpenAIAssistantsAPIRequest
            The request to retrieve attachments from.

        Returns
        -------
        List[Attachment]
            The attachments retrieved from the request.
        """
        attachments = []
        if request.attachments:
            for file_id in request.attachments:
                oai_file = await self.client.files.retrieve(file_id)
                attachments.append(
                     self._create_attachment_from_fileobject(oai_file)
                )
        return attachments

    def _parse_single_message(self, message: Message):
        """
        Parses a message from the OpenAI Assistants API
        and returns the content items within the message
        along with any annotations.

        Parameters
        ----------
        message : Message
            The message to parse.

        Returns
        -------
        List[MessageContentItemBase]
            The content items within the message.
        """
        ret_content = []

        # for each content item in the message
        for ci in message.content:
                match ci:
                    case TextContentBlock():
                        text_ci = OpenAITextMessageContentItem(
                            value=ci.text.value,
                            agent_capability_category = AgentCapabilityCategories.OPENAI_ASSISTANTS
                        )
                        for annotation in ci.text.annotations:
                            match annotation:
                                case FilePathAnnotation():
                                    file_an = OpenAIFilePathMessageContentItem(
                                        file_id=annotation.file_path.file_id,
                                        start_index=annotation.start_index,
                                        end_index=annotation.end_index,
                                        text=annotation.text,
                                        agent_capability_category = AgentCapabilityCategories.OPENAI_ASSISTANTS
                                    )
                                    text_ci.annotations.append(file_an)
                                case FileCitationAnnotation():
                                    file_cit = OpenAIFilePathMessageContentItem(
                                        file_id=annotation.file_citation.file_id,
                                        start_index=annotation.start_index,
                                        end_index=annotation.end_index,
                                        text=annotation.text,
                                        agent_capability_category = AgentCapabilityCategories.OPENAI_ASSISTANTS
                                    )
                                    text_ci.annotations.append(file_cit)
                        ret_content.append(text_ci)
                    case ImageFileContentBlock():
                        ci_img = OpenAIImageFileMessageContentItem(
                            file_id=ci.image_file.file_id,
                            agent_capability_category = AgentCapabilityCategories.OPENAI_ASSISTANTS
                        )
                        ret_content.append(ci_img)
                    case ImageURLContentBlock():
                        ci_img_url = OpenAIImageFileMessageContentItem(
                            file_url=ci.image_url.url,
                            agent_capability_category = AgentCapabilityCategories.OPENAI_ASSISTANTS
                        )
                        ret_content.append(ci_img_url)
        return ret_content

    def _parse_messages(self, messages: SyncCursorPage[Message]):
        """
        Parses the messages from the OpenAI API.

        Parameters
        ----------
        messages : SyncCursorPage[Message]
            The messages to parse.

        Returns
        -------
        List[MessageContentItemBase]
            The content items within the messages.
        """
        ret_content = []
        for msg in messages:
            ret_content.extend(self._parse_single_message(msg))
        return ret_content

    async def _aparse_messages(self, messages: AsyncCursorPage[Message]):
        """
        Parses the messages from the OpenAI API Asynchronously.

        Parameters
        ----------
        messages : AsyncCursorPage[Message]
            The messages to parse.

        Returns
        -------
        List[MessageContentItemBase]
            The content items within the messages
        """
        ret_content = []
        for msg in messages.data:
            ret_content.extend(self._parse_single_message(msg))
        return ret_content

    def _parse_run_steps(self, run_steps: SyncCursorPage[RunStep]):
        """
        Parses the run steps from the OpenAI API.

        Parameters
        ----------
        run_steps : AsyncCursorPage[RunStep]
            The run steps to parse.

        Returns
        -------
        List[AnalysisResult]
            The analysis results from the run steps.
        """
        analysis_results = []
        for rs in run_steps:
            analysis_result = self._parse_single_run_step(rs)
            if analysis_result is not None:
                analysis_results.append(analysis_result)
        return analysis_results

    async def _aparse_run_steps(self, run_steps: AsyncCursorPage[RunStep]):
        """
        Parses the run steps from the OpenAI API.

        Parameters
        ----------
        run_steps : AsyncCursorPage[RunStep]
            The run steps to parse.

        Returns
        -------
        List[AnalysisResult]
            The analysis results from the run steps.
        """
        analysis_results = []
        for rs in run_steps:
            analysis_result = self._parse_single_run_step(rs)
            if analysis_result is not None:
                analysis_results.append(analysis_result)
        return analysis_results

    def _parse_single_run_step(self, run_step: RunStep):
        """
        Parses a single run step from the OpenAI API.

        Parameters
        ----------
        run_step : RunStep
            The run step to parse.

        Returns
        -------
        AnalysisResult
            The analysis result from the run step.
        OR None
            If the run step does not contain a tool call
            to the code interpreter tool.
        """
        sd = run_step.step_details
        if sd.type == "tool_calls":
            tool_call_detail = sd.tool_calls
            for details in tool_call_detail:
                if isinstance(details, CodeInterpreterToolCall):
                    result = AnalysisResult(
                        tool_name= details.type,
                        agent_capability_category= AgentCapabilityCategories.OPENAI_ASSISTANTS
                    )
                    result.tool_input += details.code_interpreter.input  # Source code
                    for output in details.code_interpreter.outputs:  # Tool execution output
                        if hasattr(output, 'image') and output.image:
                            result.tool_output += "# Generated image file: " + output.image.file_id
                        elif hasattr(output, 'logs') and output.logs:
                            result.tool_output += output.logs
                    return result
        return None

