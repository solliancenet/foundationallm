from foundationallm.models.orchestration import MessageHistoryItem, OrchestrationRequestBase
from foundationallm.models.metadata import DataSourceMetadata

from typing import List

class CompletionRequest(OrchestrationRequestBase):
    data_source_metadata: DataSourceMetadata
    message_history: List[MessageHistoryItem] = list()