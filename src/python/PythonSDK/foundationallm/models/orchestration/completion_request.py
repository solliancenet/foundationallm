from .orchestration_request_base import OrchestrationRequestBase
from .message_history_item import MessageHistoryItem
from typing import List

class CompletionRequest(OrchestrationRequestBase):
    message_history: List[MessageHistoryItem] = list()