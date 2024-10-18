import json
import time
from openai import AsyncAssistantEventHandler, AsyncAzureOpenAI
from openai.types.beta import AssistantStreamEvent
from openai.types.beta.threads import Text, TextDelta
from openai.types.beta.threads.runs import RunStep, RunStepDelta
from typing_extensions import override
from foundationallm.models.orchestration import CompletionResponse
from foundationallm.models.services import OpenAIAssistantsAPIRequest
from foundationallm.operations import OperationsManager
from foundationallm.services import ImageService
from foundationallm.utils import OpenAIAssistantsHelpers

class OpenAIAssistantAsyncEventHandler(AsyncAssistantEventHandler):

    def __init__(self, client: AsyncAzureOpenAI, operations_manager: OperationsManager, request: OpenAIAssistantsAPIRequest, image_service: ImageService):
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
        self.image_service = image_service
        self.client = client

    @override
    async def on_event(self, event: AssistantStreamEvent) -> None:
        if event.event == "thread.run.created":
            self.assistant_id = event.data.assistant_id
            self.thread_id = event.data.thread_id
        elif event.event == "thread.run.requires_action":
            await self.on_requires_action(event.data.id)
        elif event.event == "thread.run.step.created":
            self.run_steps[event.data.id] = event.data
        elif event.event == "thread.run.step.completed":
            details = event.data.step_details
            if details and details.type == "tool_calls":
                for tool in details.tool_calls or []:
                    if tool.type == "code_interpreter" and tool.code_interpreter and tool.code_interpreter.input and tool.code_interpreter.input.endswith(tuple(self.stop_tokens)):
                        self.run_steps[event.data.id] = event.data # Overwrite the run step with the final version.    
                        await self.update_state_api_analysis_results()
                    if tool.type ==  "function":
                        self.run_steps[event.data.id] = event.data
                        await self.update_state_api_analysis_results()
        elif event.event == "thread.message.created":
            self.messages[event.data.id] = event.data
        elif event.event == "thread.message.completed":
            self.messages[event.data.id] = event.data # Overwrite the message with the final version.
            await self.update_state_api_content()
        elif "failed" in event.event:
            print(f'{event.event} ({event.data.id}): {event.data.last_error.message}')
            if event.event == "thread.run.failed":
                raise Exception(event.data.last_error.message)
            elif event.event == "thread.run.step.failed":
                pass
                # Don't end the run, but print out the error for the logs.

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

    async def on_requires_action(self, run_id: str):
        max_steps = 20
        count = 0
        while count < max_steps:
            run = await self.client.beta.threads.runs.retrieve(run_id = run_id, thread_id = self.thread_id)
            count += 1
            print(f'Poll {count} run status: {run.status}')
            if run.status == "requires_action":
                tool_responses = []
                if (run.required_action.type == "submit_tool_outputs" and run.required_action.submit_tool_outputs.tool_calls is not None):
                        # Loop through each tool in the required action section
                        for call in run.required_action.submit_tool_outputs.tool_calls:
                            # Get data from the tool
                            if call.type == "function":
                                if call.function.name == "generate_image":
                                    try:                                        
                                        tool_response = await self.image_service.agenerate_image(**json.loads(call.function.arguments))                                       
                                        tool_responses.append(
                                            {
                                                "tool_call_id": call.id,
                                                "output": json.dumps(tool_response)
                                            }
                                        )                                        
                                    except Exception as ex:                                       
                                        print(f'Error getting tool response: {ex}')
                                        break
                try:
                    await self.client.beta.threads.runs.submit_tool_outputs(
                        run_id = run.id,
                        thread_id = self.thread_id,
                        tool_outputs = tool_responses,
                        stream = True
                    )
                except Exception as e:
                    print(f'Error submitting tool outputs: {e}')
                    continue
            if run.status == "failed":
                print('Run failed')
                break
            if run.status == "completed":
                break
            time.sleep(3)

    async def update_state_api_analysis_results(self):
        self.interim_result.analysis_results = [] # Clear the analysis results list before adding new results.
        for k, v in self.run_steps.items():
            if not v:
                continue
            analysis_result = OpenAIAssistantsHelpers.parse_run_step(v)
            if analysis_result:
                self.interim_result.analysis_results.append(analysis_result)
        await self.operations_manager.set_operation_result(self.request.operation_id, self.request.instance_id, self.interim_result)
        
    async def update_state_api_content(self):
        self.interim_result.content = [] # Clear the content list before adding new messages.
        for k, v in self.messages.items():
            content_items = OpenAIAssistantsHelpers.parse_message(v)
            self.interim_result.content.extend(content_items)

        await self.operations_manager.set_operation_result(self.request.operation_id, self.request.instance_id, self.interim_result)
