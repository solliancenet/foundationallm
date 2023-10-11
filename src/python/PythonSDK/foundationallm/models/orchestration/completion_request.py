from foundationallm.models.orchestration import MessageHistoryItem, OrchestrationRequestBase
from foundationallm.models.metadata import DataSourceMetadata

from typing import List

class CompletionRequest(OrchestrationRequestBase):
    data_source: DataSourceMetadata
    message_history: List[MessageHistoryItem] = list()