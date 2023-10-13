from typing import List

from foundationallm.models.orchestration import (Agent, DataSource, LanguageModel, 
                                                 MessageHistoryItem, OrchestrationRequest)

class CompletionRequest(OrchestrationRequest):
    """
    Orchestration completion request.
    """
    agent: Agent
    data_source: DataSource
    language_model: LanguageModel
    message_history: List[MessageHistoryItem] = list()