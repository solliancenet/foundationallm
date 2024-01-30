from typing import Any
import pytest
from foundationallm.config import Configuration
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.models.metadata import Agent, DataSource
from foundationallm.langchain.data_sources.search_service import SearchServiceConfiguration
from foundationallm.models.language_models import EmbeddingModel, LanguageModelType, LanguageModelProvider, LanguageModel
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import SearchServiceAgent

@pytest.fixture
def test_config():
    return Configuration()                         

@pytest.fixture
def test_sotu_completion_request():
     req = CompletionRequest(
         user_prompt="Who is the the longest serving Senate Leader in history?",
         agent=Agent(
             name="sotu",
             type="search-service",
             description="Useful for searching for information about the State of the Union address from February 2023.",
             prompt_prefix="You are a political science professional named Baldwin. You are responsible for answering questions regarding the February 2023 State of the Union Address.\nAnswer only questions about the February 2023 State of the Union address. Do not make anything up. Check your answers before replying.\nProvide concise answers that are polite and professional."
         ),
         data_sources=[DataSource(
             name="sotu-ds",
             type="search-service",
             description="Transcript from the February 2023 State of the Union Address",
             configuration=SearchServiceConfiguration(
                 configuration_type="search_service",
                 endpoint="FoundationaLLM:CognitiveSearch:EndPoint",
                 key_secret="FoundationaLLM:CognitiveSearch:Key",
                 index_name="sotu-index",
                 embedding_field_name="Embedding",
                 text_field_name="Text",
                 top_n = 3
             )
         )],
         language_model=LanguageModel(
            type=LanguageModelType.OPENAI,
            provider=LanguageModelProvider.MICROSOFT,
            temperature=0,
            use_chat=True),
         embedding_model = EmbeddingModel(
            type = LanguageModelType.OPENAI,
            provider = LanguageModelProvider.MICROSOFT,
            deployment = 'FoundationaLLM:AzureOpenAI:API:Embeddings:DeploymentName',
            model = 'FoundationaLLM:AzureOpenAI:API:Embeddings:ModelName',
            chunk_size = 10
        ), 
        message_history=[]
     )
     return req

@pytest.fixture
def test_sotu_llm(test_sotu_completion_request, test_config):
    model_factory = LanguageModelFactory(language_model=test_sotu_completion_request.language_model, config = test_config)
    return model_factory.get_llm()

class SearchServiceAgentTests:    
   def test_agent_should_return_valid_response_sotu_txt(self, test_sotu_completion_request, test_sotu_llm, test_config):        
        agent = SearchServiceAgent(completion_request=test_sotu_completion_request,llm=test_sotu_llm, config=test_config)
        completion_response = agent.run(prompt=test_sotu_completion_request.user_prompt)
        print(completion_response.completion)
        assert "mcconnell" in completion_response.completion.lower()
