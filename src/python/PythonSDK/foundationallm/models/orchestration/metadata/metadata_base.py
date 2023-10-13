from pydantic import BaseModel

class MetadataBase(BaseModel):
    """Metadata model base class."""
    name: str
    type: str
    description: str