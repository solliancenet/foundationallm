"""
Encapsulates properties useful for calling the OpenAI Assistants API.
"""
from typing import List, Optional, Union
from pydantic import BaseModel
from foundationallm.models.orchestration import (
    AnalysisResult,
    FunctionResult,
    OpenAIFilePathMessageContentItem,
    OpenAIImageFileMessageContentItem,
    OpenAITextMessageContentItem
)

class OpenAIAssistantsAPIResponse(BaseModel):
    """
    Encapsulates properties after parsing the output of the OpenAI Assistants API.
       
    """
    document_id: Optional[str]
    content: Optional[
        List[
            Union[
                OpenAIFilePathMessageContentItem,
                OpenAIImageFileMessageContentItem,
                OpenAITextMessageContentItem
            ]
        ]
    ] = []
    analysis_results: Optional[List[AnalysisResult]] = []
    function_results: Optional[List[FunctionResult]] = []
    completion_tokens: Optional[int] = 0
    prompt_tokens: Optional[int] = 0
    total_tokens: Optional[int] = 0
    errors: Optional[List[str]] = []
