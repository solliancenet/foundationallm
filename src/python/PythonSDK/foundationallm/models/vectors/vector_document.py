from typing import Any, List
from langchain_core.documents import Document

class VectorDocument(Document):

    id : str
    score: float
