import pytest
from functools import partial
from unittest.mock import patch
from foundationallm.config import Configuration
from foundationallm.models.metadata import KnowledgeManagementAgent as KnowledgeManagementAgentMetadata
from foundationallm.models.metadata import ConversationHistory, Gatekeeper
from foundationallm.models.orchestration import KnowledgeManagementCompletionRequest
from foundationallm.models.language_models import LanguageModelType, LanguageModelProvider, LanguageModel
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import KnowledgeManagementAgent
from foundationallm.resources import ResourceProvider

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def test_resource_provider(test_config):
    return ResourceProvider(config=test_config)

@pytest.fixture
def test_azure_ai_search_service_completion_request():
     req = KnowledgeManagementCompletionRequest(
         user_prompt=""" 
            When did the State of the Union Address take place?
         """,
         agent=KnowledgeManagementAgentMetadata(
            name="sotu",
            type="knowledge-management",
            description="Knowledge Management Agent that queries the State of the Union speech transcript.",
            language_model=LanguageModel(
                type=LanguageModelType.OPENAI,
                provider=LanguageModelProvider.MICROSOFT,
                temperature=0,
                use_chat=True
            ),
            indexing_profile_object_ids=["/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Vectorization/indexingprofiles/sotu-index"],
            text_embedding_profile_object_id="/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Vectorization/textembeddingprofiles/AzureOpenAI_Embedding",
            prompt_object_id="/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Prompt/prompts/sotu",
            sessions_enabled=True,
            conversation_history = ConversationHistory(enabled=True, max_history=5),
            gatekeeper=Gatekeeper(use_system_setting=True, options=["ContentSafety", "Presidio"])
         ),
         message_history = [
            {
                "sender": "User",
                "text": "What is your name?"
            },
            {
                "sender": "Assistant",
                "text": "My name is Baldwin."
            }
        ]
     )
     return req

@pytest.fixture
def test_llm(test_azure_ai_search_service_completion_request, test_config, test_resource_provider):
    model_factory = LanguageModelFactory(language_model=test_azure_ai_search_service_completion_request.agent.language_model, config = test_config)
    return model_factory.get_llm()

class KnowledgeManagementAgentTests:
         
    def test_azure_ai_search_azure_authentication(self, test_azure_ai_search_service_completion_request):
        config = Configuration()
        resource_provider = ResourceProvider(config)
  
        # Save the original methods without side effects
        og_config_get_value_fn = config.get_value
        og_rp_get_resource_fn = resource_provider.get_resource
  
        # side effect function for when Foundationallm:Test:AuthenticationType:AzureIdentity is 
        #   requested, return AzureIdentity (this is a faux app settings key for this test)
        def config_get_value_side_effect(key):  
            if key == "Foundationallm:Test:AuthenticationType:AzureIdentity":  
                return "AzureIdentity"  
            else:  
                return og_config_get_value_fn(key)  
  
        # side effect function to override indexing profile authentication type key
        #  to Foundationallm:Test:AuthenticationType:AzureIdentity (faux app settings key)
        def resource_provider_get_resource_side_effect(object_id):  
            if "indexingprofiles" in object_id:  
                # Modify the authentication_type directly for this test  
                resource = og_rp_get_resource_fn(object_id)  
                resource.configuration_references.authentication_type = "Foundationallm:Test:AuthenticationType:AzureIdentity"  
                return resource  
            else:  
                return og_rp_get_resource_fn(object_id)  
  
        # Patch the methods on Configuration and ResourceProvider with the side effect functions
        with patch.object(Configuration, 'get_value', side_effect=config_get_value_side_effect):              
            with patch.object(ResourceProvider, 'get_resource', side_effect=resource_provider_get_resource_side_effect):               
                model_factory = LanguageModelFactory(language_model=test_azure_ai_search_service_completion_request.agent.language_model, config = config)
                llm = model_factory.get_llm()
                agent = KnowledgeManagementAgent(completion_request=test_azure_ai_search_service_completion_request, llm=llm, config=config, resource_provider=resource_provider)
                completion_response = agent.run(prompt=test_azure_ai_search_service_completion_request.user_prompt)
                print(completion_response.completion)
                assert completion_response.completion is not None
                
             
    def test_azure_ai_search_service_agent_initializes(self, test_llm, test_config, test_azure_ai_search_service_completion_request, test_resource_provider):
        agent = KnowledgeManagementAgent(completion_request=test_azure_ai_search_service_completion_request, llm=test_llm, config=test_config, resource_provider=test_resource_provider)              
        assert agent is not None

    def test_azure_ai_search_gets_completion(self, test_llm, test_config, test_azure_ai_search_service_completion_request, test_resource_provider):
        agent = KnowledgeManagementAgent(completion_request=test_azure_ai_search_service_completion_request, llm=test_llm, config=test_config, resource_provider=test_resource_provider)
        completion_response = agent.run(prompt=test_azure_ai_search_service_completion_request.user_prompt)
        print(completion_response.completion)
        assert completion_response.completion is not None

    def test_azure_ai_search_gets_correct_completion(self, test_llm, test_config, test_azure_ai_search_service_completion_request, test_resource_provider):
        agent = KnowledgeManagementAgent(completion_request=test_azure_ai_search_service_completion_request, llm=test_llm, config=test_config, resource_provider=test_resource_provider)
        completion_response = agent.run(prompt=test_azure_ai_search_service_completion_request.user_prompt)
        print(completion_response.completion)
        assert "february" in completion_response.completion.lower() or "2023" in completion_response.completion
