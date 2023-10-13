from typing import List
from foundationallm.models.orchestration import MessageHistoryItem, OrchestrationRequest
from foundationallm.models.orchestration.metadata import Agent
from foundationallm.models.orchestration.metadata import DataSource
from foundationallm.models.orchestration.metadata import LanguageModel


class CompletionRequest(OrchestrationRequest):
    """
    Orchestration completion request.
    """
    agent: Agent = None
    data_source: DataSource = None
    language_model: LanguageModel = None
    message_history: list[MessageHistoryItem] = list()