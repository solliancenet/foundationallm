import pytest
from foundationallm.config import Configuration
from foundationallm.hubs.agent import AgentRepository, AgentResolver, AgentHubRequest


@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def agent_repository(test_config):
    return AgentRepository(config=test_config)

@pytest.fixture
def agent_resolver(test_config, agent_repository):
    return AgentResolver(repository=agent_repository, config=test_config)

class AgentResolverTests:
    """
    AgentResolverTests is responsible for testing the selection of the best-fit
        agent to respond to a user prompt.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    def test_agent_resolver_should_return_single_agent(self, agent_resolver):                
        agent_request = AgentHubRequest(
            user_prompt="What is FoundationaLLM?",
            message_history=None
        )        
        agent_response = agent_resolver.resolve(agent_request)
        assert agent_response.agent is not None
        
    def test_agent_resolver_should_return_default_agent(self, agent_resolver):                
        agent_request = AgentHubRequest(
            user_prompt="What is FoundationaLLM?",
            message_history=None
        )        
        agent_response = agent_resolver.resolve(agent_request)
        assert agent_response.agent.name == "default"
        
    def test_agent_resolver_should_return_weather_agent(self, agent_resolver):                
        agent_request = AgentHubRequest(
            user_prompt="How much days of hail were there in the month of October?",
            message_history=None
        )        
        agent_response = agent_resolver.resolve(agent_request)
        assert agent_response.agent.name == "weather"