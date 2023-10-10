from pydantic import BaseModel

class MessageHistoryItem(BaseModel):
    sender: str
    text: str

    def __init__(self, sender: str, text: str):
        self.sender = sender
        self.text = text