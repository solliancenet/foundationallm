from openai import AsyncAssistantEventHandler
from openai.types.beta import AssistantStreamEvent
from openai.types.beta.threads import Text, TextDelta
from openai.types.beta.threads.runs import RunStep, RunStepDelta
from typing_extensions import override
from foundationallm.models.orchestration.completion_response import CompletionResponse
from foundationallm.operations import OperationsManager
from foundationallm.models.services import OpenAIAssistantsAPIRequest
from foundationallm.utils import OpenAIAssistantsHelpers

class OpenAIAssistantAsyncEventHandler(AsyncAssistantEventHandler):

    def __init__(self, operations_manager: OperationsManager, request: OpenAIAssistantsAPIRequest):
        super().__init__()
        self.operations_manager = operations_manager
        self.request = request
        self.run_steps = {}
        self.messages = {}
        self.stop_tokens = [".", ",", ":", ";", "\n", " ", ")"] # Use stop tokens to determine when to write to State API.
        self.assistant_id = ''
        self.thread_id = ''
        self.interim_result = CompletionResponse(
            id = request.document_id,
            operation_id = request.operation_id,
            user_prompt = request.user_prompt,
            content = [],
            analysis_results = []
        )

    @override
    async def on_event(self, event: AssistantStreamEvent) -> None:
        if event.event == "thread.run.created":
            self.assistant_id = event.data.assistant_id
            self.thread_id = event.data.thread_id
        elif event.event == "thread.run.step.created":
            self.run_steps[event.data.id] = event.data
        elif event.event == "thread.run.step.completed":

            details = event.data.step_details
            if details and details.type == "tool_calls":
                for tool in details.tool_calls or []:
                    if tool.type == "code_interpreter" and tool.code_interpreter and tool.code_interpreter.input and tool.code_interpreter.input.endswith(tuple(self.stop_tokens)):
                        self.run_steps[event.data.id] = event.data # Overwrite the run step with the final version.
                        await self.update_state_api_analysis_results()
        elif event.event == "thread.message.created":
            self.messages[event.data.id] = event.data
        elif event.event == "thread.message.completed":
            self.messages[event.data.id] = event.data # Overwrite the message with the final version.
            await self.update_state_api_content()

    @override
    async def on_text_delta(self, delta: TextDelta, snapshot: Text) -> None:
        if snapshot.value.endswith(tuple(self.stop_tokens)): # Use stop tokens to determine when to write to State API.
            await self.update_state_api_content()

    @override
    async def on_run_step_delta(self, delta: RunStepDelta, snapshot: RunStep) -> None:
        details = delta.step_details
        if details and details.type == "tool_calls":
            for tool in details.tool_calls or []:
                if tool.type == "code_interpreter" and tool.code_interpreter and tool.code_interpreter.input and tool.code_interpreter.input.endswith(tuple(self.stop_tokens)):
                    self.run_steps[snapshot.id] = snapshot
                    await self.update_state_api_analysis_results()

    async def update_state_api_analysis_results(self):
        self.interim_result.analysis_results = [] # Clear the analysis results list before adding new results.
        for k, v in self.run_steps.items():
            analysis_result = OpenAIAssistantsHelpers.parse_run_step(v)
            if analysis_result:
                self.interim_result.analysis_results.append(analysis_result)

        await self.operations_manager.set_operation_result(self.request.operation_id, self.request.instance_id, self.interim_result)
        await self.operations_manager.set_operation_result(self.request.operation_id, self.request.instance_id, self.interim_result)

    async def update_state_api_content(self):
        self.interim_result.content = [] # Clear the content list before adding new messages.
        for k, v in self.messages.items():
            content_items = OpenAIAssistantsHelpers.parse_message(v)
            self.interim_result.content.extend(content_items)

        await self.operations_manager.set_operation_result(self.request.operation_id, self.request.instance_id, self.interim_result)
