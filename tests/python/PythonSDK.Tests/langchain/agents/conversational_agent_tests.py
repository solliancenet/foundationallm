import pytest
from foundationallm.config import Configuration
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.models.metadata import Agent
from foundationallm.models.language_models import (
    LanguageModelType,
    LanguageModelProvider,
    LanguageModel,
)
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import ConversationalAgent
import spacy


@pytest.fixture
def test_config():
    return Configuration()


@pytest.fixture
def test_websearch_completion_request():
    req = CompletionRequest(
        user_prompt="Who won the 2023 Cricket World Cup final match and when was it?",
        agent=Agent(
            name="currentevents",
            type="web-search",
            description="Answers users' questions about current events dating after September 2021",
            prompt_prefix="You are an Internet-connected intelligent answering agent named Sandy. Do not make anything up. Use the provided web search facility.",
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
def test_websearch_llm(test_websearch_completion_request, test_config):
    model_factory = LanguageModelFactory(
        language_model=test_websearch_completion_request.language_model,
        config=test_config,
    )
    return model_factory.get_llm()


@pytest.fixture
def spacy_similarity_model():
    return spacy.load("en_core_web_sm")


class ConversationalAgentTests:
    def test_agent_should_return_valid_response_cricket_websearch(
        self,
        test_websearch_completion_request,
        test_websearch_llm,
        spacy_similarity_model,
    ):
        agent = ConversationalAgent(
            completion_request=test_websearch_completion_request,
            llm=test_websearch_llm,
            config=test_config,
        )
        completion_response = agent.run(
            prompt=test_websearch_completion_request.user_prompt
        )
        model_answer = spacy_similarity_model(completion_response.completion)
        ref_answer = spacy_similarity_model(
            "Australia won the 2023 Cricket World Cup final match. The match was held on November 19, 2023."
        )
        assert ref_answer.similarity(model_answer) > 0.8