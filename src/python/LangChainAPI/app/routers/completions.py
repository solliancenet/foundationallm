"""
The API endpoint for returning the completion from the LLM for the specified user prompt.
"""
import asyncio
import json
from typing import Optional, List
from fastapi import (
    APIRouter,
    BackgroundTasks,
    Body,
    Depends,
    Header,
    HTTPException,
    Request,
    Response,
    status
)
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.operations import (
    LongRunningOperation,
    LongRunningOperationLogEntry,
    OperationStatus
)
from foundationallm.models.orchestration import (
    CompletionRequestBase,
    CompletionResponse,
    OpenAITextMessageContentItem
)
from foundationallm.models.agents import KnowledgeManagementCompletionRequest
from foundationallm.operations import OperationsManager
from foundationallm.langchain.orchestration import OrchestrationManager
from foundationallm.telemetry import Telemetry
from app.dependencies import handle_exception, validate_api_key_header

# Initialize telemetry logging
logger = Telemetry.get_logger(__name__)
tracer = Telemetry.get_tracer(__name__)

# Initialize API routing
router = APIRouter(
    prefix='/instances/{instance_id}',
    tags=['completions'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

async def resolve_completion_request(request_body: dict = Body(...)) -> CompletionRequestBase:
    agent_type = request_body.get("agent", {}).get("type", None)

    match agent_type:
        case "knowledge-management":
            request = KnowledgeManagementCompletionRequest(**request_body)
            request.agent.type = agent_type
            return request
        case _:
            raise ValueError(f"Unsupported agent type: {agent_type}")

@router.post(
    '/async-completions',
    summary = 'Submit an async completion request.',
    status_code = status.HTTP_202_ACCEPTED,
    responses = {
        202: {'description': 'Completion request accepted.'},
    }
)
async def submit_completion_request(
    raw_request: Request,
    response: Response,
    background_tasks: BackgroundTasks,
    instance_id: str,
    completion_request: CompletionRequestBase = Depends(resolve_completion_request),
    x_user_identity: Optional[str] = Header(None)
) -> LongRunningOperation:
    """
    Initiates the creation of a completion response in the background.

    Returns
    -------
    CompletionOperation
        Object containing the operation ID and status.
    """
    with tracer.start_as_current_span('submit_completion_request') as span:
        try:
            # Get the operation_id from the completion request.
            operation_id = completion_request.operation_id

            span.set_attribute('operation_id', operation_id)
            span.set_attribute('instance_id', instance_id)
            span.set_attribute('user_identity', x_user_identity)

            location = f'{raw_request.base_url}instances/{instance_id}/async-completions/{operation_id}/status'
            response.headers['location'] = location

            # Create an operations manager to create the operation.
            operations_manager = OperationsManager(raw_request.app.extra['config'])
            # Submit the completion request operation to the state API.
            operation = await operations_manager.create_operation_async(operation_id, instance_id, x_user_identity)

            # Start a background task to perform the completion request.
            background_tasks.add_task(
                create_completion_response,
                operation_id,
                instance_id,
                completion_request,
                raw_request.app.extra['config'],
                operations_manager,
                x_user_identity
            )

            # Return the long running operation object.
            return operation

        except Exception as e:
            handle_exception(e)

async def create_completion_response(
    operation_id: str,
    instance_id: str,
    completion_request: KnowledgeManagementCompletionRequest,
    configuration: Configuration,
    operations_manager: OperationsManager,
    x_user_identity: Optional[str] = Header(None)
):
    """
    Generates the completion response for the specified completion request.
    """
    with tracer.start_as_current_span(f'create_completion_response') as span:
        try:
            span.set_attribute('operation_id', operation_id)
            span.set_attribute('instance_id', instance_id)
            span.set_attribute('user_identity', x_user_identity)

            # Change the operation status to 'InProgress' using an async task.
            await operations_manager.update_operation_async(
                operation_id,
                instance_id,
                status = OperationStatus.INPROGRESS,
                status_message = 'Operation state changed to in progress.',
                user_identity = x_user_identity
            )

            # Create the user identity object from the x_user_identity header.            
            user_identity_dict = json.loads(x_user_identity)
            user_identity = UserIdentity(**user_identity_dict)
          
            # Create an orchestration manager to process the completion request.
            orchestration_manager = OrchestrationManager(
                completion_request = completion_request,
                configuration = configuration,
                operations_manager = operations_manager,
                instance_id = instance_id,
                user_identity = user_identity
            )
            # Await the completion response from the orchestration manager.
            completion_response = await orchestration_manager.invoke_async(completion_request)

            # Send the completion response to the State API and mark the operation as completed.
            await asyncio.gather(
                operations_manager.set_operation_result_async(
                    operation_id = operation_id,
                    instance_id = instance_id,
                    completion_response = completion_response),
                operations_manager.update_operation_async(
                    operation_id = operation_id,
                    instance_id = instance_id,
                    status = OperationStatus.COMPLETED,
                    status_message = f'Operation {operation_id} completed successfully.',
                    user_identity = x_user_identity
                )
            )
        except Exception as e:
            # Send the completion response to the State API and mark the operation as failed.
            completion_response = CompletionResponse(
                operation_id = operation_id,
                user_prompt = completion_request.user_prompt,
                content = [
                    OpenAITextMessageContentItem(
                        text = "A problem on my side prevented me from responding."
                    )
                ],
                errors=[f'{e}']
            )
            await asyncio.gather(
                operations_manager.set_operation_result_async(
                    operation_id = operation_id,
                    instance_id = instance_id,
                    completion_response = completion_response),
                operations_manager.update_operation_async(
                    operation_id = operation_id,
                    instance_id = instance_id,
                    status = OperationStatus.FAILED,
                    status_message = f'{e}',
                    user_identity = x_user_identity
                )
            )

@router.get(
    '/async-completions/{operation_id}/status',
    summary = 'Retrieve the status of the completion request operation with the specified operation ID.',
    responses = {
        200: {'description': 'The operation status was retrieved successfully.'},
        404: {'description': 'The specified operation was not found.'}
    }
)
async def get_operation_status(
    raw_request: Request,
    instance_id: str,
    operation_id: str
) -> LongRunningOperation:
    with tracer.start_as_current_span(f'get_operation_status') as span:
        # Create an operations manager to get the operation status.
        operations_manager = OperationsManager(raw_request.app.extra['config'])

        try:
            span.set_attribute('operation_id', operation_id)
            span.set_attribute('instance_id', instance_id)

            operation = await operations_manager.get_operation_async(
                operation_id,
                instance_id
            )

            if operation is None:
                raise HTTPException(status_code=404, detail=f"An operation with the id '{operation_id}' does not exist.")

            return operation
        except HTTPException as he:
            handle_exception(he, he.status_code)
        except Exception as e:
            handle_exception(e)

@router.get(
    '/async-completions/{operation_id}/result',
    summary = 'Retrieve the completion result of the operation with the specified operation ID.',
    responses = {
        200: {'description': 'The operation result was retrieved successfully.'},
        404: {'description': 'The specified operation or its result was not found.'}
    }
)
async def get_operation_result(
    raw_request: Request,
    instance_id: str,
    operation_id: str
) -> CompletionResponse:
    with tracer.start_as_current_span(f'get_operation_result') as span:
        # Create an operations manager to get the operation result.
        operations_manager = OperationsManager(raw_request.app.extra['config'])

        try:
            span.set_attribute('operation_id', operation_id)
            span.set_attribute('instance_id', instance_id)

            completion_response = await operations_manager.get_operation_result_async(
                operation_id,
                instance_id
            )

            if completion_response is None:
                raise HTTPException(status_code=404)

            return completion_response
        except Exception as e:
            handle_exception(e)

@router.get(
    '/async-completions/{operation_id}/logs',
    summary = 'Retrieve the log of operational steps for the specified operation ID.',
    responses = {
        200: {'description': 'The operation log was retrieved successfully.'},
        404: {'description': 'The specified operation or its log was not found.'}
    }
)
async def get_operation_logs(
    raw_request: Request,
    instance_id: str,
    operation_id: str
) -> List[LongRunningOperationLogEntry]:
    with tracer.start_as_current_span(f'get_operation_log') as span:
        # Create an operations manager to get the operation log.
        operations_manager = OperationsManager(raw_request.app.extra['config'])

        try:
            span.set_attribute('operation_id', operation_id)
            span.set_attribute('instance_id', instance_id)

            log = await operations_manager.get_operation_logs_async(
                operation_id,
                instance_id
            )

            if log is None:
                raise HTTPException(status_code=404)

            return log
        except Exception as e:
            handle_exception(e)
