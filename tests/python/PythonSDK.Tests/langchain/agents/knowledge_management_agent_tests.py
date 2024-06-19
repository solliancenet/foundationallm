import pytest
from functools import partial
from unittest.mock import patch
from foundationallm.config import Configuration
from foundationallm.models.agents import (
    KnowledgeManagementAgent,
    AgentVectorizationSettings,
    AgentGatekeeperSettings,
    AgentConversationHistorySettings
)
from foundationallm.models.agents import KnowledgeManagementCompletionRequest
from foundationallm.models.orchestration import OrchestrationSettings
from foundationallm.models.language_models import LanguageModelType, LanguageModelProvider
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import LangChainKnowledgeManagementAgent

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def test_azure_ai_search_service_completion_request():
     req = KnowledgeManagementCompletionRequest(
         user_prompt=""" 
            When did the State of the Union Address take place?
         """,
         agent=KnowledgeManagementAgent(
            name="sotu",
            type="knowledge-management",
            description="Knowledge Management Agent that queries the State of the Union speech transcript.",
            orchestration_settings=OrchestrationSettings(
                orchestrator = "LangChain",
                agent_parameters={},
                endpoint_configuration={
                    "auth_type": "token",
                    "provider": "microsoft",
                    "endpoint": "https://test-openai.openai.azure.com/",
                    "api_version": "2024-02-15-preview"
                },
                model_parameters={
                    "deployment_name": "completions"
                }
            ),
            vectorization=AgentVectorizationSettings(
                indexing_profile_object_ids=["/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Vectorization/indexingprofiles/sotu-index"],
                text_embedding_profile_object_id="/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Vectorization/textembeddingprofiles/AzureOpenAI_Embedding",
            ),           
            prompt_object_id="/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Prompt/prompts/sotu",
            sessions_enabled=True,
            conversation_history = AgentConversationHistorySettings(enabled=True, max_history=5),
            gatekeeper = AgentGatekeeperSettings(use_system_setting=True, options=["ContentSafety", "Presidio"])
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
def test_azure_ai_search_service_completion_request_zoo():
     req = KnowledgeManagementCompletionRequest(
         user_prompt=""" 
            In what year was the Zoo founded?
         """,
         agent=KnowledgeManagementAgent(
            name="sdwa",
            type="knowledge-management",
            description="Zoo Journal Index",
            vectorization=AgentVectorizationSettings(
                indexing_profile_object_id="/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Vectorization/indexingprofiles/AzureAISearch_CPTEST",
                text_embedding_profile_object_id="/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Vectorization/textembeddingprofiles/AzureOpenAI_Embedding",
            ),           
            prompt_object_id="/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Prompt/prompts/sdzwa",
            sessions_enabled=True,
            conversation_history = AgentConversationHistorySettings(enabled=True, max_history=5),
            gatekeeper=AgentGatekeeperSettings(use_system_setting=True, options=["ContentSafety", "Presidio"])
         ),
         message_history = [
            {
                "sender": "User",
                "text": "What is your name?"
            },
            {
                "sender": "Assistant",
                "text": "My name is Khalil."
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
  
        # Save the original methods without side effects
        og_config_get_value_fn = config.get_value
  
        # side effect function for when Foundationallm:Test:AuthenticationType:AzureIdentity is 
        #   requested, return AzureIdentity (this is a faux app settings key for this test)
        def config_get_value_side_effect(key):  
            if key == "Foundationallm:Test:AuthenticationType:AzureIdentity":  
                return "AzureIdentity"  
            else:  
                return og_config_get_value_fn(key)  
    
        # Patch the methods on Configuration and ResourceProvider with the side effect functions
        with patch.object(Configuration, 'get_value', side_effect=config_get_value_side_effect):              
            model_factory = LanguageModelFactory(language_model=test_azure_ai_search_service_completion_request.agent.language_model, config = config)
            llm = model_factory.get_llm()
            agent = KnowledgeManagementAgent(completion_request=test_azure_ai_search_service_completion_request, llm=llm, config=config)
            completion_response = agent.run(prompt=test_azure_ai_search_service_completion_request.user_prompt)
            print(completion_response.completion)
            assert completion_response.completion is not None
                
             
    def test_azure_ai_search_service_agent_initializes(self, test_llm, test_config, test_azure_ai_search_service_completion_request):
        agent = KnowledgeManagementAgent(completion_request=test_azure_ai_search_service_completion_request, llm=test_llm, config=test_config)              
        assert agent is not None

    def test_azure_ai_search_gets_completion(self, test_llm, test_config, test_azure_ai_search_service_completion_request):
        agent = KnowledgeManagementAgent(completion_request=test_azure_ai_search_service_completion_request, llm=test_llm, config=test_config)
        completion_response = agent.run(prompt=test_azure_ai_search_service_completion_request.user_prompt)
        print(completion_response.completion)
        assert completion_response.completion is not None

    def test_azure_ai_search_gets_correct_completion(self, test_llm, test_config, test_azure_ai_search_service_completion_request):
        agent = KnowledgeManagementAgent(completion_request=test_azure_ai_search_service_completion_request, llm=test_llm, config=test_config)
        completion_response = agent.run(prompt=test_azure_ai_search_service_completion_request.user_prompt)
        print(completion_response.completion)
        assert "february" in completion_response.completion.lower() or "2023" in completion_response.completion
    
    def test_azure_ai_search_gets_citations(self, test_llm, test_config, test_azure_ai_search_service_completion_request_zoo):
        agent = KnowledgeManagementAgent(completion_request=test_azure_ai_search_service_completion_request_zoo, llm=test_llm, config=test_config)
        completion_response = agent.run(prompt=test_azure_ai_search_service_completion_request_zoo.user_prompt)
        print(completion_response.citations)
        assert completion_response.citations is not None and len(completion_response.citations) > 0
