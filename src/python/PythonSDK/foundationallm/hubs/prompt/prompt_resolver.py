from foundationallm.hubs.prompt import PromptHubRequest, PromptHubResponse
from foundationallm.hubs import Resolver

class PromptResolver(Resolver):
    """The PromptResolver class is responsible for resolving a request to a metadata value."""

    def resolve(self, request:PromptHubRequest) -> PromptHubResponse:        
        return PromptHubResponse(prompts=self.repository.get_metadata_values(request.agent_name)) 