from typing import Any
import pytest
from foundationallm.config import Configuration
from foundationallm.models import ListOption
from foundationallm.models.orchestration import MessageHistoryItem
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.models.metadata import Agent, DataSource
from foundationallm.langchain.data_sources.blob import BlobStorageConfiguration
from foundationallm.models.language_models import EmbeddingModel, LanguageModelType, LanguageModelProvider, LanguageModel
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import BlobStorageAgent

@pytest.fixture
def test_config():
    return Configuration()                         

@pytest.fixture
def test_zoo_completion_request():
     req = CompletionRequest(
         user_prompt="How many species of ungulates live in Africa's savannas?",
         agent=Agent(
             name="sdzwa",
             type="blob-storage",
             description="Provides details about the San Diego Zoo Wildlife Alliance originating from the 2022 and 2023 issues of the journal.",
             prompt_prefix="You are the San Diego Zoo assistant named Sandy. You are responsible for answering questions related to the San Diego Zoo that is contained in the journal publications. Only answer questions that relate to the Zoo and journal content. Do not make anything up. Use only the data provided."
         ),
         data_sources=[DataSource(
             name="sdzwa-ds",
             type="blob-storage",
             description="Information about the San Diego Zoo publications.",
             configuration=BlobStorageConfiguration(
                 connection_string_secret="FoundationaLLM:BlobStorageMemorySource:BlobStorageConnection",
                 container="sdzwa-source",
                 files = [
                     "SDZWA-Journal-July-2023.pdf",
                     "SDZWA-Journal-March-2023.pdf",
                     "SDZWA-Journal-May-2023.pdf",
                     "SDZWA-Journal-November-2023.pdf",
                     "SDZWA-Journal-September-2023.pdf"
                 ]
             )
         )],
         language_model=LanguageModel(
            type=LanguageModelType.OPENAI,
            provider=LanguageModelProvider.MICROSOFT,
            temperature=0,
            use_chat=True),
        message_history=[]
     )
     return req

@pytest.fixture
def test_zoo_llm(test_zoo_completion_request, test_config):
    model_factory = LanguageModelFactory(language_model=test_zoo_completion_request.language_model, config = test_config)
    return model_factory.get_llm()

@pytest.fixture
def test_fllm_completion_request():
    req = CompletionRequest(
        user_prompt="What is FoundationaLLM?",
        agent=Agent(
            name="default",
            type="blob-storage",
            description="Useful for answering questions from users.",
            prompt_prefix="You are an analytic agent named Khalil that helps people find information about FoundationaLLM.\nProvide concise answers that are polite and professional.\nDo not include in your answers things you are not sure about."
        ),
        data_sources=[DataSource(
            name="about-foundationallm",
            type="blob-storage",
            description="Information about FoundationaLLM.",            
            configuration=BlobStorageConfiguration(
                configuration_type="blob_storage",
                connection_string_secret="FoundationaLLM:DataSources:AboutFoundationaLLM:BlobStorage:ConnectionString",
                container="foundationallm-source",
                files = ["about.txt"]
            )
        )],
        language_model=LanguageModel(
            type=LanguageModelType.OPENAI,
            provider=LanguageModelProvider.MICROSOFT,
            temperature=0,
            use_chat=True
        ),
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
def test_fllm_llm(test_fllm_completion_request, test_config):
    model_factory = LanguageModelFactory(language_model=test_fllm_completion_request.language_model, config = test_config)
    return model_factory.get_llm()

class BlobStorageAgentTests:    
   def test_agent_should_return_valid_response_fllm_txt(self, test_fllm_completion_request, test_fllm_llm, test_config):        
        agent = BlobStorageAgent(completion_request=test_fllm_completion_request,llm=test_fllm_llm, config=test_config)
        completion_response = agent.run(prompt=test_fllm_completion_request.user_prompt)
        print(completion_response.completion)
        assert "copilot" in completion_response.completion.lower()

# TESTS ARE COMMENTED OUT DUE TO THE LENGTH OF TIME IT TAKES TO VECTORIZE THE PDF FILES
#   def test_agent_should_return_valid_response_ungulatescount_pdf(self, test_zoo_completion_request, test_zoo_llm, test_config):        
#       agent = BlobStorageAgent(completion_request=test_zoo_completion_request,llm=test_zoo_llm, config=test_config)
#       completion_response = agent.run(prompt=test_zoo_completion_request.user_prompt)
#       print(completion_response.completion)
#       assert "40" in completion_response.completion.lower()
#       
#   def test_agent_should_return_valid_response_journal_cadence_pdf(self, test_zoo_completion_request, test_zoo_llm, test_config):        
#       test_zoo_completion_request.user_prompt = "How often is the Journal published?"        
#       agent = BlobStorageAgent(completion_request=test_zoo_completion_request,llm=test_zoo_llm, config=test_config)
#       completion_response = agent.run(prompt=test_zoo_completion_request.user_prompt)
#       print(completion_response.completion)
#       # test for bi-monthly and bimonthly
#       assert "bimonthly" in completion_response.completion.lower().replace("-","")
#   
#   def test_agent_should_return_valid_response_beisaoryx_pdf(self, test_zoo_completion_request, test_zoo_llm, test_config):        
#       test_zoo_completion_request.user_prompt = "What is a beisa oryx?"        
#       agent = BlobStorageAgent(completion_request=test_zoo_completion_request,llm=test_zoo_llm, config=test_config)
#       completion_response = agent.run(prompt=test_zoo_completion_request.user_prompt)
#       print(completion_response.completion)
#       assert "antelope" in completion_response.completion.lower()