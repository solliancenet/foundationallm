from pydantic import BaseModel

class MessageHistoryItem(BaseModel):
    """Represents an historic message sender and text item."""
    
    sender: str
    text: str

    def __init__(self, sender: str, text: str):
        """
        Message history item
        
        Parameters
        ----------
        sender : str
            The sender of the message (e.g., "Agent", "User")
        text : str
            The message text.
        """
        
        self.sender = sender
        self.text = text