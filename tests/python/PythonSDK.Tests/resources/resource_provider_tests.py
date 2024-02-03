import pytest
from foundationallm.config import Configuration
from foundationallm.resources import ResourceProvider

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def sut(test_config):
    return ResourceProvider(config=test_config)

class ResourceProviderTests:   
           
    def test_resource_provider_initializes(self, sut):        
        assert sut is not None
        
    def test_get_resource_returns_prompt_resource(self, sut):
        resource_id ="/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Prompt/prompts/sotu"
        result = sut.get_resource(resource_id)
        assert result["name"] == "sotu"
        
    def test_get_resource_returns_indexing_profile_resource(self, sut):
        resource_id ="/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Vectorization/indexingprofiles/sotu-index"
        result = sut.get_resource(resource_id)
        assert result["name"] == "sotu-index"

    def test_get_resource_returns_embedding_profile_resource(self, sut):
        resource_id ="/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Vectorization/textembeddingprofiles/AzureOpenAI_Embedding"
        result = sut.get_resource(resource_id)
        assert result["name"] == "AzureOpenAI_Embedding"
