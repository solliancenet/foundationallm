from foundationallm.hubs.agent import AgentMetadata, AgentHubRequest, AgentHubResponse
from foundationallm.hubs import Resolver
from foundationallm.config import Configuration
from typing import List
from foundationallm.langchain.agents.resolvers.agent_resolver import FoundationaLLMAgentResolverAgent

class AgentResolver(Resolver):
    """The AgentResolver class is responsible for resolving a request to a metadata value."""
    def __init__(self, repository, config: Configuration):
        super().__init__(repository)
        self.config = config
        
    def resolve(self, request:AgentHubRequest) -> AgentHubResponse:
        resolver_agent = FoundationaLLMAgentResolverAgent(request, self.repository.get_metadata_values(), self.config)
        completion_response = resolver_agent.run(request.user_prompt)
        agent_name = completion_response.completion
        agent = self.repository.get_metadata_by_name(agent_name)
        return AgentHubResponse(agent=agent)