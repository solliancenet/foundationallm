import pytest
from foundationallm.hubs.agent import AgentHubRequest, AgentRepository
from foundationallm.config import Configuration
from foundationallm.langchain.agents.resolvers.agent_resolver import FoundationaLLMAgentResolverAgent

@pytest.fixture
def agent_request():
    return AgentHubRequest(
            user_prompt="On average, how many reports of hail occur in October?",
            message_history=None
        )

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def agent_metadata_list(test_config):
    agent_repo = AgentRepository(test_config)
    return agent_repo.get_metadata_values()

@pytest.fixture
def agent_resolver_agent(agent_request, agent_metadata_list, test_config):
    return FoundationaLLMAgentResolverAgent(agent_request, agent_metadata_list, test_config)

class FoundationaLLMAgentResolverAgentTests:
    """
    FoundationaLLMAgentResolverAgentTests is responsible for testing the selection of the best-fit
        agent to respond to a user prompt.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    
    def test_build_agent_list(self, agent_resolver_agent):
       agent_list = agent_resolver_agent.build_agent_choices_list()
       assert agent_list is not None
       
    def test_run_should_resolve_weather(self, agent_request, agent_resolver_agent):
        completion = agent_resolver_agent.run(agent_request.user_prompt)
        assert completion.completion == "weather"
        
    def test_run_should_resolve_default(self, agent_metadata_list, test_config):
        agent_request = AgentHubRequest(
            user_prompt="What is FoundationaLLM?",
            message_history=None
        )
        agent_resolver_agent = FoundationaLLMAgentResolverAgent(agent_request, agent_metadata_list, test_config)
        completion = agent_resolver_agent.run(agent_request.user_prompt)
        assert completion.completion == "default"