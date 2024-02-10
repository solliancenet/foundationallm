import pytest
from foundationallm.config import Configuration
from foundationallm.models.language_models import (
    LanguageModelType,
    LanguageModel,
    LanguageModelProvider,
)
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.tools import TypeConversionTool
from langchain.agents.agent_toolkits import create_python_agent
from langchain.tools.python.tool import PythonREPLTool
from langchain.agents.agent_types import AgentType


@pytest.fixture
def test_config():
    return Configuration()


@pytest.fixture
def test_tool_llm(test_config):
    language_model = LanguageModel(
        type=LanguageModelType.OPENAI,
        provider=LanguageModelProvider.MICROSOFT,
        temperature=0,
        use_chat=True,
    )
    llm = LanguageModelFactory(
        language_model=language_model,
        config=test_config,
    ).get_llm()
    return llm.get_completion_model(language_model)


@pytest.fixture
def python_agent(test_tool_llm):
    return create_python_agent(
        llm=test_tool_llm,
        tool=PythonREPLTool(),
        prefix="You are a helpful agent that answers whether the user's question is True or False. Two values are equivalent if they represent the same value, even if they have different types. Perform the necessary type conversions to facilitate comparison. ONLY return True or False, and nothing more.",
        agent_type=AgentType.ZERO_SHOT_REACT_DESCRIPTION,
        verbose=True,
        agent_executor_kwargs={"handle_parsing_errors": True},
    )


@pytest.fixture
def type_conversion_tool(python_agent):
    return TypeConversionTool(agent=python_agent)


class TypeConversionToolTests:
    """
    TypeConversionToolTests is responsible for testing TypeConversionTool's
        ability to answer reasonable comparison questions that may require type conversions.

    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri

    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """

    def test_same_type_and_value(self, type_conversion_tool):
        completion_response = type_conversion_tool._run("Is 3 equivalent to 3?")
        parsed_response = completion_response.strip('"').strip("'")
        assert parsed_response == "True"

    def test_different_type_and_same_value(self, type_conversion_tool):
        completion_response = type_conversion_tool._run("Is 3 equivalent to '3'?")
        parsed_response = completion_response.strip('"').strip("'")
        assert parsed_response == "True"

    def test_same_type_and_different_value(self, type_conversion_tool):
        completion_response = type_conversion_tool._run("Is 'Shoe' equivalent to 'Sock'?")
        parsed_response = completion_response.strip('"').strip("'")
        assert parsed_response == "False"

    def test_different_type_and_different_value(self, type_conversion_tool):
        completion_response = type_conversion_tool._run("Is 3 equivalent to 'Shoe'?")
        parsed_response = completion_response.strip('"').strip("'")
        assert parsed_response == "False"