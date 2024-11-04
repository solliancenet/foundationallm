"""
Class: OpenAIAssistantsApiService
Description: Integration with the OpenAI Assistants API.
"""
from openai import AsyncAzureOpenAI
from openai.pagination import AsyncCursorPage
from openai.types import FileObject
from openai.types.beta.threads import Message
from openai.types.beta.threads.message import Attachment
from openai.types.beta.threads.runs import RunStep
from foundationallm.event_handlers import OpenAIAssistantAsyncEventHandler
from foundationallm.models.orchestration import OpenAITextMessageContentItem
from foundationallm.operations import OperationsManager
from foundationallm.models.services import OpenAIAssistantsAPIRequest, OpenAIAssistantsAPIResponse
from foundationallm.services import ImageService
from foundationallm.utils import OpenAIAssistantsHelpers

class OpenAIAssistantsApiService:
    """
    Integration with the OpenAI Assistants API.
    """

    def __init__(self, azure_openai_client: AsyncAzureOpenAI, operations_manager: OperationsManager):
        """
        Initializes an OpenAI Assistants API service.

        Parameters
        ----------
        azure_openai_client : AsyncAzureOpenAI
            Async Azure OpenAI client for interacting with the OpenAI Assistants API.
        operations_manager : OperationsManager
            Operations manager for updating the operation.
        """
        self.client = azure_openai_client
        self.operations_manager = operations_manager

    async def add_thread_message_async(self, thread_id: str, role: str, content: str, attachments: list = None):
        return await self.client.beta.threads.messages.create(
            thread_id = thread_id,
            role = role,
            content = content,
            attachments = attachments
        )

    async def run_async(self, request: OpenAIAssistantsAPIRequest, image_service: ImageService) -> OpenAIAssistantsAPIResponse:
        """
        Creates an OpenAI Assistant Run and executes it asynchronously.

        Parameters
        ----------
        request : OpenAIAssistantsAPIRequest
            The request to run with the OpenAI Assistants API service.
        image_service : ImageService
            The image service to use for generating images. If None, no image generation tool will be added.

        Returns
        -------
        OpenAIAssistantsAPIResponse
            The response parsed from the OpenAI Assistants API service response.
        """
        # Process file attachments and assign tools
        attachments = await self._get_request_attachments_async(request)

        # Add User prompt to the thread
        try:
            message = await self.add_thread_message_async(
                thread_id = request.thread_id,
                role = "user",
                content = request.user_prompt,
                attachments = attachments
            )
        except Exception as e:
            error_message = f"Error adding user prompt message to thread: {e}"
            return OpenAIAssistantsAPIResponse(
                document_id = request.document_id,
                content = [
                    OpenAITextMessageContentItem(
                        text = "A problem on my side prevented me from responding."
                    )
                ],
                errors = [error_message]
            )
        
        # Create an image generation tool for the assistant
        if image_service is not None:
            image_generation_tool = {"type": "function", "function": image_service.get_function_definition(function_name='generate_image')}

            # Add the image generation tool to the assistant.
            assistant = await self.client.beta.assistants.retrieve(assistant_id=request.assistant_id)
            tools = assistant.tools

            # If the tools collection already contains the function, remove it
            for tool in tools:
                if tool.type == 'function' and tool.function.name == "generate_image":
                    tools.remove(tool)

            tools.append(image_generation_tool)
            await self.client.beta.assistants.update(assistant_id=request.assistant_id, tools=tools)

        # Create and execute the run
        run = None
        async with self.client.beta.threads.runs.stream(
            thread_id = request.thread_id,
            assistant_id = request.assistant_id,
            event_handler = OpenAIAssistantAsyncEventHandler(self.client, self.operations_manager, request, image_service),
            additional_instructions = "If you generate an image, return the image inline using markdown, along with a detailed description of it.\n\nIMPORTANT: Never display the image more than once in your response!"
        ) as stream:
            await stream.until_done()
            run = await stream.get_final_run()

        if run.status != "completed":
            run = await self.client.beta.threads.runs.retrieve(run_id = run.id, thread_id = request.thread_id)

        if run.status == "failed":
            return OpenAIAssistantsAPIResponse(
                document_id = request.document_id,
                content = [
                    OpenAITextMessageContentItem(
                        text = "A problem on my side prevented me from responding."
                    )
                ],
                errors = [
                    run.last_error.message
                ]
            )
        
        # Retrieve the steps from the run_steps for the analysis
        run_steps = await self.client.beta.threads.runs.steps.list(
          thread_id = request.thread_id,
          run_id = run.id
        )

        analysis_results, function_results = await self._parse_run_steps_async(run_steps.data)

        # Retrieve the messages in the thread after the prompt message was appended.
        messages = await self.client.beta.threads.messages.list(
            thread_id=request.thread_id, order="asc", after=message.id
        )

        content = await self._parse_messages_async(messages)

        return OpenAIAssistantsAPIResponse(
            document_id = request.document_id,
            content = content,
            analysis_results = analysis_results,
            function_results = function_results,
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

    async def _get_request_attachments_async(self, request: OpenAIAssistantsAPIRequest):
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

    async def _parse_messages_async(self, messages: AsyncCursorPage[Message]):
        """
        Parses the messages from the OpenAI API.

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

    async def _parse_run_steps_async(self, run_steps: AsyncCursorPage[RunStep]):
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
        function_results = []
        for rs in run_steps:
            a_results, f_results = OpenAIAssistantsHelpers.parse_run_step(rs)
            analysis_results.extend(a_results)
            function_results.extend(f_results)
        return analysis_results, function_results
