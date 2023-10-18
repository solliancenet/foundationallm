from typing import List, Optional
from foundationallm.models.orchestration import MessageHistoryItem, OrchestrationRequest
from foundationallm.models.metadata import Agent
from foundationallm.models.metadata import DataSource
from foundationallm.models.metadata import LanguageModel

class CompletionRequest(OrchestrationRequest):
    """
    Orchestration completion request.
    """
    agent: Agent = None
    data_source: Optional[DataSource] = None
    language_model: LanguageModel = None
    message_history: Optional[List[MessageHistoryItem]] = list()