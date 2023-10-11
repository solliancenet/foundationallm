from typing import Union

from .orchestration_response_base import OrchestrationResponseBase

class CompletionResponse(OrchestrationResponseBase):
    completion: Union[str, set]
    #user_prompt_embedding: list(float) = None