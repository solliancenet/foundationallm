"""
Class: OpenAIAssistantsApiService
Description: Integration with the OpenAI Assistants API.
"""
from typing import Union
from openai import AsyncAzureOpenAI, AzureOpenAI
from openai.pagination import AsyncCursorPage, SyncCursorPage
from openai.types import FileObject
from openai.types.beta.threads import Message
from openai.types.beta.threads.message import Attachment
from openai.types.beta.threads.runs import RunStep
from foundationallm.event_handlers import OpenAIAssistantAsyncEventHandler, OpenAIAssistantEventHandler
from foundationallm.operations import OperationsManager
from foundationallm.models.services import OpenAIAssistantsAPIRequest, OpenAIAssistantsAPIResponse
from foundationallm.utils import OpenAIAssistantsHelpers

class OpenAIAssistantsApiService:
    """
    Integration with the OpenAI Assistants API.
    """

    def __init__(self, azure_openai_client: Union[AzureOpenAI, AsyncAzureOpenAI], operations_manager: OperationsManager):
        """
        Initializes an OpenAI Assistants API service.

        Parameters
        ----------
        azure_openai_client : AzureOpenAI
            Azure OpenAI client for interacting with the OpenAI Assistants API.
        operations_manager : OperationsManager
            Operations manager for updating the operation.
        """
        self.client = azure_openai_client
        self.operations_manager = operations_manager

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

        Returns
        -------
        OpenAIAssistantsAPIResponse
            The response parsed from the OpenAI Assistants API service response.
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
        run = None
        with self.client.beta.threads.runs.stream(
            thread_id = request.thread_id,
            assistant_id = request.assistant_id,
            event_handler = OpenAIAssistantEventHandler(self.operations_manager, request)
        ) as stream:
            stream.until_done()
            run = stream.get_final_run()

        # Retrieve the steps from the run_steps for the analysis
        run_steps = self.client.beta.threads.runs.steps.list(
          thread_id = request.thread_id,
          run_id = run.id
        )

        analysis_results = self._parse_run_steps(run_steps.data)

        # Retrieve the messages in the thread after the prompt message was appended.
        messages = self.client.beta.threads.messages.list(
            thread_id = request.thread_id, order="asc", after=message.id
        )

        content = self._parse_messages(messages)

        return OpenAIAssistantsAPIResponse(
            document_id = request.document_id,
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
        run = None
        async with self.client.beta.threads.runs.stream(
            thread_id = request.thread_id,
            assistant_id = request.assistant_id,
            event_handler = OpenAIAssistantAsyncEventHandler(self.operations_manager, request)
        ) as stream:
            await stream.until_done()
            run = await stream.get_final_run()

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
            document_id = request.document_id,
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
            ret_content.extend(OpenAIAssistantsHelpers.parse_message(msg))
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
            ret_content.extend(OpenAIAssistantsHelpers.parse_message(msg))
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
            analysis_result = OpenAIAssistantsHelpers.parse_run_step(rs)
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
            analysis_result = OpenAIAssistantsHelpers.parse_run_step(rs)
            if analysis_result is not None:
                analysis_results.append(analysis_result)
        return analysis_results
