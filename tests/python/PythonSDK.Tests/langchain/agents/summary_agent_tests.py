import os
import pytest
from unittest.mock import MagicMock
from foundationallm.config import Configuration
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.models.language_models import (
    LanguageModel,
    LanguageModelProvider,
    LanguageModelType,
)
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.models.metadata import Agent
from foundationallm.langchain.agents import SummaryAgent, summary_agent
import spacy
from azure.identity import DefaultAzureCredential
from azure.storage.blob import BlobServiceClient


@pytest.fixture
def blob_client():
    return BlobServiceClient(
        account_url=f'https://{os.getenv("INTEGRATION_TESTS_SA", "")}.blob.core.windows.net',
        credential=DefaultAzureCredential(),
    )


@pytest.fixture
def load_sdzwa_text(blob_client):
    blob_client = blob_client.get_blob_client(
        container=os.getenv("INTEGRATION_TESTS_CONTAINER"), blob="sdzwa.txt"
    )
    return blob_client.download_blob().readall().decode("utf-8")


@pytest.fixture
def load_sdzwa_reference_summary(blob_client):
    blob_client = blob_client.get_blob_client(
        container=os.getenv("INTEGRATION_TESTS_CONTAINER"), blob="sdzwa-ref-summary.txt"
    )
    return blob_client.download_blob().readall().decode("utf-8")


@pytest.fixture
def test_config():
    return Configuration()


@pytest.fixture
def test_config_lower_token_limit():
    mock_config = MagicMock()
    configuration = Configuration()
    mock_config.get_value.side_effect = (
        lambda key: 500
        if key == "FoundationaLLM:LangChain:Summary:MaxTokens"
        else configuration.get_value(key)
    )
    return mock_config


@pytest.fixture
def test_zoo_completion_request():
    req = CompletionRequest(
        user_prompt="Summarize the article you read.",
        agent=Agent(
            name="sdzwa",
            type="summarizer",
            description='Provides details about the "Shell Game" article in the September 2023 edition of the journal.',
            prompt_prefix="""
             You are the San Diego Zoo assistant named Sandy. You are responsible for writing a summary for the provided document. Do not make anything up. Use only the data provided.
             
             {text}
             """,
        ),
        language_model=LanguageModel(
            type=LanguageModelType.OPENAI,
            provider=LanguageModelProvider.MICROSOFT,
            temperature=0,
            use_chat=True,
        ),
        message_history=[],
    )
    return req


@pytest.fixture
def test_zoo_llm(test_zoo_completion_request, test_config):
    model_factory = LanguageModelFactory(
        language_model=test_zoo_completion_request.language_model, config=test_config
    )
    return model_factory.get_llm()


@pytest.fixture
def spacy_similarity_model():
    return spacy.load("en_core_web_sm")


class SummaryAgentTests:
    def test_completion(
        self,
        test_zoo_completion_request,
        test_zoo_llm,
        test_config,
        load_sdzwa_text,
        load_sdzwa_reference_summary,
        spacy_similarity_model,
    ):
        """
        This test verifies that the StuffDocumentsChain appropriately summarizes the provided text (735 tokens).

        The model's output is judged according to its similarity with a reference summary.

        A passing test requires 80% similarity.
        """
        summary_agent = SummaryAgent(
            test_zoo_completion_request, test_zoo_llm, test_config
        )
        ref_answer = spacy_similarity_model(load_sdzwa_reference_summary)
        model_completion = spacy_similarity_model(
            summary_agent.run(load_sdzwa_text).completion
        )
        assert ref_answer.similarity(model_completion) > 0.8

    def test_map_reduce(
        self,
        test_zoo_completion_request,
        test_zoo_llm,
        test_config_lower_token_limit,
        load_sdzwa_text,
        load_sdzwa_reference_summary,
        spacy_similarity_model,
    ):
        """
        This test verifies that the MapReduceDocumentsChain appropriately summarizes the provided text (735 tokens).

        Even though the smallest models have a token limit of 4K, using a smaller document reduces the test time.

        The model's output is judged according to its similarity with a reference summary.

        A passing test requires 80% similarity.
        """
        summary_agent = SummaryAgent(
            test_zoo_completion_request, test_zoo_llm, test_config_lower_token_limit
        )
        ref_answer = spacy_similarity_model(load_sdzwa_reference_summary)
        model_completion = spacy_similarity_model(
            summary_agent.run(load_sdzwa_text).completion
        )
        assert ref_answer.similarity(model_completion) > 0.8