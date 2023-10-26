import pytest
from foundationallm.config import Configuration
from foundationallm.langchain.agents.resolvers import ResolverConfigurationRepository
from langchain.prompts import PromptTemplate
from langchain.base_language import BaseLanguageModel

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def repo(test_config):
    return ResolverConfigurationRepository(test_config)

class ResolverConfigurationRepositoryTests:
    """
    The ResolverConfigurationRepositoryTests class is responsible for testing the retrieval of internal agent configuration details.
    Including: 
        agent_resolver
   
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """ 
  
    def test_get_agent_resolver_prompt(self, repo):        
        prompt = repo.get_agent_resolver_prompt()        
        assert prompt is not None

    def test_get_agent_resolver_prompt_should_return_langchain_prompt_template(self, repo):
        prompt = repo.get_agent_resolver_prompt()        
        assert isinstance(prompt, PromptTemplate)

    def test_get_agent_resolver_llm_details(self, repo):       
        llm = repo.get_agent_resolver_llm_details()  
        assert llm is not None
        
    def test_get_agent_resolver_llm_details_should_return_langchain_baselanguagemodel(self, repo):
        llm = repo.get_agent_resolver_llm_details()
        assert isinstance(llm, BaseLanguageModel)
        
