"""
Encapsulates properties useful for calling the OpenAI Assistants API.
"""
from typing import List, Optional, Union
from pydantic import BaseModel
from foundationallm.models.orchestration import (
    AnalysisResult,
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
    ]
    analysis_results: Optional[List[AnalysisResult]]
    completion_tokens: Optional[int]
    prompt_tokens: Optional[int]
    total_tokens: Optional[int]
