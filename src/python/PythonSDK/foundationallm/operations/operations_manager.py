import os
import requests
from typing import List
from foundationallm.config import Configuration
from foundationallm.models.operations import (
    LongRunningOperation,
    LongRunningOperationLogEntry,
    OperationStatus
)
from foundationallm.models.orchestration import CompletionResponse

class OperationsManager():
    """
    Class for managing long running operations via calls to the StateAPI.
    """
    def __init__(self, config: Configuration):
        self.config = config
        # Retrieve the State API configuration settings.
        env = os.environ.get('FOUNDATIONALLM_ENV', 'prod')

        self.state_api_url = config.get_value('FoundationaLLM:APIEndpoints:StateAPI:Essentials:APIUrl').rstrip('/')
        self.state_api_key = config.get_value('FoundationaLLM:APIEndpoints:StateAPI:Essentials:APIKey')
        self.verify_certs = False if env == 'dev' else True
        
    async def create_operation(
        self,
        operation_id: str,
        instance_id: str) -> LongRunningOperation:
        """
        Creates a background operation by settings its initial state through the State API.

        POST {state_api_url}/instances/{instanceId}/operations/{operationId} -> LongRunningOperation
        
        Parameters
        ----------
        operation_id : str
            The unique identifier for the operation.
        instance_id : str
            The unique identifier for the FLLM instance.
        
        Returns
        -------
        LongRunningOperation
            Object representing the operation.
        """               
        try:
            headers = {
                "x-api-key": self.state_api_key,
                "charset":"utf-8",
                "Content-Type":"application/json"
            }
            
            # Call the State API to create a new operation.
            r = requests.post(
                f'{self.state_api_url}/instances/{instance_id}/operations/{operation_id}',
                headers=headers,
                verify=self.verify_certs
            )

            if r.status_code != 200:
                raise Exception(f'An error occurred while creating the operation {operation_id}: ({r.status_code}) {r.text}')

            return r.json()
        except Exception as e:
            raise e

    async def update_operation(self,
        operation_id: str,
        instance_id: str,
        status: OperationStatus,
        status_message: str) -> LongRunningOperation:
        """
        Updates the state of a background operation through the State API.

        PUT {state_api_url}/instances/{instanceId}/operations/{operationId} -> LongRunningOperation
        
        Parameters
        ----------
        operation : LongRunningOperation
            The operation to update.
        instance_id : str
            The unique identifier for the FLLM instance.
        status: OperationStatus
            The new status to assign to the operation.
        status_message: str
            The message to associate with the new status.
        
        Returns
        -------
        LongRunningOperation
            Object representing the operation.
        """
        operation = LongRunningOperation(
            operation_id=operation_id,
            status=status,
            status_message=status_message
        )
        
        try:
            # Call the State API to create a new operation.
            headers = {
                "x-api-key": self.state_api_key,
                "charset":"utf-8",
                "Content-Type":"application/json"
            }

            r = requests.put(
                f'{self.state_api_url}/instances/{instance_id}/operations/{operation_id}',
                json=operation.model_dump(exclude_unset=True),
                headers=headers,
                verify=self.verify_certs
            )

            if r.status_code == 404:
                return None

            if r.status_code != 200:
                raise Exception(f'An error occurred while updating the status of operation {operation_id}: ({r.status_code}) {r.text}')

            return r.json()
        except Exception as e:
            raise e

    async def get_operation(
        self,
        operation_id: str,
        instance_id: str) -> LongRunningOperation:
        """
        Retrieves the state of a background operation through the State API.

        GET {state_api_url}/instances/{instanceId}/operations/{operationId} -> LongRunningOperation
        
        Parameters
        ----------
        operation_id : str
            The unique identifier for the operation.
        instance_id : str
            The unique identifier for the FLLM instance.
        
        Returns
        -------
        LongRunningOperation
            Object representing the operation.
        """
        try:
            # Call the State API to create a new operation.
            headers = {
                "x-api-key": self.state_api_key,
                "charset":"utf-8",
                "Content-Type":"application/json"
            }

            print(f'status endpoint: {self.state_api_url}/instances/{instance_id}/operations/{operation_id}')

            r = requests.get(
                f'{self.state_api_url}/instances/{instance_id}/operations/{operation_id}',
                headers=headers,
                verify=self.verify_certs
            )

            if r.status_code == 404:
                return None

            if r.status_code != 200:
                raise Exception(f'An error occurred while retrieving the status of the operation {operation_id}: ({r.status_code}) {r.text}')

            return r.json()
        except Exception as e:
            raise e

    async def set_operation_result(
        self,
        operation_id: str,
        instance_id: str,
        completion_response: CompletionResponse):
        """
        Sets the result of a completion operation through the State API.

        PUT {state_api_url}/instances/{instanceId}/operations/{operationId}/result -> CompletionResponse
        
        Parameters
        ----------
        operation_id : str
            The unique identifier for the operation.
        instance_id : str
            The unique identifier for the FLLM instance.
        completion_response : CompletionResponse
            The result of the operation.
        """
        try:
            # Call the State API to create a new operation.
            headers = {
                "x-api-key": self.state_api_key,
                "charset":"utf-8",
                "Content-Type":"application/json"
            }

            r = requests.post(
                f'{self.state_api_url}/instances/{instance_id}/operations/{operation_id}/result',
                json=completion_response.model_dump(),
                headers=headers,
                verify=self.verify_certs
            )

            if r.status_code == 404:
                return None

            if r.status_code != 200:
                raise Exception(f'An error occurred while submitting the result of operation {operation_id}: ({r.status_code}) {r.text}')

        except Exception as e:
            raise e

    async def get_operation_result(
        self,
        operation_id: str,
        instance_id: str) -> CompletionResponse:
        """
        Retrieves the result of an async completion operation through the State API.

        GET {state_api_url}/instances/{instanceId}/operations/{operationId}/result -> CompletionResponse
        
        Parameters
        ----------
        operation_id : str
            The unique identifier for the operation.
        instance_id : str
            The unique identifier for the FLLM instance.
        
        Returns
        -------
        CompletionResponse
            Object representing the operation result.
        """
        try:
            # Call the State API to create a new operation.
            headers = {
                "x-api-key": self.state_api_key,
                "charset":"utf-8",
                "Content-Type":"application/json"
            }

            r = requests.get(
                f'{self.state_api_url}/instances/{instance_id}/operations/{operation_id}/result',
                headers=headers,
                verify=self.verify_certs
            )

            if r.status_code == 404:
                return None

            if r.status_code != 200:
                raise Exception(f'An error occurred while retrieving the result of operation {operation_id}: ({r.status_code}) {r.text}')

            return r.json()
        except Exception as e:
            raise e

    async def get_operation_logs(
        self,
        operation_id: str,
        instance_id: str) -> List[LongRunningOperationLogEntry]:
        """
        Retrieves a list of log entries for an async operation through the State API.

        GET {state_api_url}/instances/{instanceId}/operations/{operationId}/log -> List[LongRunningOperationLogEntry]
        
        Parameters
        ----------
        operation_id : str
            The unique identifier for the operation.
        instance_id : str
            The unique identifier for the FLLM instance.
        
        Returns
        -------
        List[LongRunningOperationLogEntry]
            List of log entries for the operation.
        """
        try:
            # Call the State API to create a new operation.
            headers = {
                "x-api-key": self.state_api_key,
                "charset":"utf-8",
                "Content-Type":"application/json"
            }

            r = requests.get(
                f'{self.state_api_url}/instances/{instance_id}/operations/{operation_id}/logs',
                headers=headers,
                verify=self.verify_certs
            )

            if r.status_code == 404:
                return None

            if r.status_code != 200:
                raise Exception(f'An error occurred while retrieving the log of steps for the operation {operation_id}: ({r.status_code}) {r.text}')

            return r.json()
        except Exception as e:
            raise e
