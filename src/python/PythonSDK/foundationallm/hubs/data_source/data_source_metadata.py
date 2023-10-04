from typing import List, Optional
from foundationallm.hubs import Metadata
from foundationallm.hubs.data_source import UnderlyingImplementation


class DataSourceMetadata(Metadata):
    name: str
    description: str
    underlying_implementation: UnderlyingImplementation
    few_shot_examples: Optional[List[str]] = None
