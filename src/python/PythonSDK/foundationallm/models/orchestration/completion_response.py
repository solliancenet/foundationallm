from typing import List, Optional, Union
from pydantic import BaseModel

from .analysis_result import AnalysisResult
from .citation import Citation
from .openai_image_file_message_content_item import OpenAIImageFileMessageContentItem
from .openai_text_message_content_item import OpenAITextMessageContentItem

class CompletionResponse(BaseModel):
    """
    Response from a language model.
    """
    id: Optional[str] = None
    operation_id: str
    user_prompt: str
    full_prompt: Optional[str] = None
    completion: Optional[Union[str, set, List[str]]] = None
    content: Optional[
        List[
            Union[
                OpenAIImageFileMessageContentItem, 
                OpenAITextMessageContentItem
            ]
        ]
    ] = None
    analysis_results: Optional[List[AnalysisResult]] = []
    citations: Optional[List[Citation]] = []
    user_prompt_embedding: Optional[List[float]] = []
    prompt_tokens: int = 0
    completion_tokens: int = 0
    total_tokens: int = 0
    total_cost: float = 0.0
