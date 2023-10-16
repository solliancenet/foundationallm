from foundationallm.hubs.agent import AgentMetadata, AgentHubRequest, AgentHubResponse
from foundationallm.hubs import Resolver
from typing import List

class AgentResolver(Resolver):
    """The AgentResolver class is responsible for resolving a request to a metadata value."""
    def resolve(self, request:AgentHubRequest) -> AgentHubResponse:               
        return AgentHubResponse(agent=self.repository.get_metadata_values()[0])