from .request_base import RequestBase
from .message_history_item import MessageHistoryItem

class CompletionRequest(RequestBase):
    message_history: list[MessageHistoryItem] = list()