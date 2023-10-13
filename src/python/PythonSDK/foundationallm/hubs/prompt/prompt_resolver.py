from foundationallm.hubs.prompt import PromptMetadata
from foundationallm.hubs import Resolver
from typing import List

class PromptResolver(Resolver):
    """The PromptResolver class is responsible for resolving a request to a metadata value."""

    def resolve(self, request, metadata_values)->List[PromptMetadata]:
        # This should use some logic to resolve the request to a metadata
        # For simplicity, returning the Default metadata        
        return [metadata_values[4]]