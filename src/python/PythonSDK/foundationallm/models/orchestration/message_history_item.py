from pydantic import BaseModel

class MessageHistoryItem(BaseModel):  
    """
        Represents an historic message sender and text item.
        
        Parameters
        ----------
        sender : str
            The sender of the message (e.g., "Agent", "User")
        text : str
            The message text.
    """    
    sender: str
    text: str

    