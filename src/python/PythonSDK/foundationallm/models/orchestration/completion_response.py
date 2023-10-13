from typing import Union

from .orchestration_response import OrchestrationResponse

class CompletionResponse(OrchestrationResponse):
    """
    Response from a language model.
    """
    
    completion: Union[str, set]
    user_prompt_embedding: list(float) = None
    
    def __init__(self, completion: str, user_prompt: str, prompt_tokens: int = 0, completion_tokens: int = 0, total_tokens: int = 0, total_cost: float = 0.0, user_prompt_embedding: list(float) = None):
        """
        Initialize a completion response

        Parameters
        ----------
        completion : Union[str, set]
            The completion response from the language model.
        user_prompt : str
            The user prompt the language model responded to.
        prompt_tokens : int
            The number of tokens in the prompt.
        completion_tokens : int
            The number of tokens in the completion.
        total_tokens: int
            The total number of tokens.
        total_cost : float
            The total cost of executing the completion operation.
        user_prompt_embedding : list(float)
            User prompt embedding.
        """
        
        self.completion = completion
        self.user_prompt = user_prompt
        self.prompt_tokens = prompt_tokens
        self.completion_tokens = completion_tokens
        self.total_tokens = total_tokens
        self.total_cost = total_cost
        self.user_prompt_embedding = user_prompt_embedding