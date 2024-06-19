"""
Encapsulates properties agent vectorization settings.
"""
from typing import Optional, List
from pydantic import BaseModel

class AgentVectorizationSettings(BaseModel):
    """
    Encapsulates properties for agent vectorization settings.
    """
    dedicated_pipeline: Optional[bool] = False
    data_source_object_id: Optional[str] = None
    indexing_profile_object_ids: Optional[List[str]] = None
    text_embedding_profile_object_id: Optional[str] = None
    text_partitioning_profile_object_id: Optional[str] = None
    vectorization_data_pipeline_object_id: Optional[str] = None
