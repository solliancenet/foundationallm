from abc import ABC
from foundationallm.hubs import Resolver, Repository

class HubBase(ABC):
    """The HubBase class is responsible for managing and resolving requests."""
    
    def __init__(self, resolver: Resolver, repository: Repository):
        self.resolver = resolver
        self.repository = repository        

    def resolve_request(self, request):
        metadata_values = self.repository.get_metadata_values()
        return self.resolver.resolve(request, metadata_values)