import pytest
import os
from unittest.mock import MagicMock
from foundationallm.config import Configuration
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.models.metadata import Agent, DataSource
from foundationallm.models.language_models import (
    LanguageModelType,
    LanguageModelProvider,
    LanguageModel,
)
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import SqlDbAgent
from foundationallm.langchain.data_sources.sql import SQLDatabaseConfiguration
from foundationallm.config import Context


@pytest.fixture
def test_config():
    mock_config = MagicMock()
    configuration = Configuration()
    mock_config.get_value.side_effect = (
        lambda key: "Password.1!!"
        if key == "TEST_MSSQL_PW_KEY"
        else configuration.get_value(key)
    )
    return mock_config


@pytest.fixture
def sql_data_source_config():
    return SQLDatabaseConfiguration(
        dialect="mssql",
        password_secret_setting_key_name="TEST_MSSQL_PW_KEY",
        host=os.getenv("TEST_MSSQL_DB_HOST", "localhost"),
        port=os.getenv("TEST_MSSQL_DB_PORT", 1433),
        database_name=os.getenv("TEST_MSSQL_DB_NAME", "WideWorldImporters"),
        username=os.getenv("TEST_MSSQL_DB_USERNAME", "sa"),
        include_tables=["CustomerTransactions", "Customers"],
        schema="Sales",
        use_row_level_security=False,
    )


@pytest.fixture
def test_sql_completion_request(sql_data_source_config):
    req = CompletionRequest(
        user_prompt="What is the ID of the customer who had the second highest total spent value over all transactions, excluding tax?",
        agent=Agent(
            name="wwi-sql",
            type="sql",
            description="Answers questions based on the provided SQL Database",
            prompt_prefix="You are an answering machine that generates and executes queries targeting the WideWorldImporters Microsoft SQL Server database. Do not make anything up; if you cannot answer a question, let the user know politely.",
        ),
        language_model=LanguageModel(
            type=LanguageModelType.OPENAI,
            provider=LanguageModelProvider.MICROSOFT,
            temperature=0,
            use_chat=True,
        ),
        data_source=DataSource(
            name="wwi-sql",
            type="sql",
            description="Useful for when you need to answer questions about Wide World Importers.",
            data_description="Wide World Importers SQL Database",
            configuration=sql_data_source_config,
        ),
        message_history=[],
    )
    return req


@pytest.fixture
def test_sql_llm(test_sql_completion_request, test_config):
    model_factory = LanguageModelFactory(
        language_model=test_sql_completion_request.language_model, config=test_config
    )
    return model_factory.get_llm()


class SqlDbAgentTests:
    def test_sql_db_qa(self, test_sql_completion_request, test_sql_llm, test_config):
        """
        This test verifies the functionality of SqlDbAgent on a sample Microsoft SQL Server database.

        The test is limited to the CustomerTransactions and Customers tables in the Sales schema.
        
        It uses a custom output parser to facilitate schema discovery.
        """
        agent = SqlDbAgent(
            completion_request=test_sql_completion_request,
            llm=test_sql_llm,
            config=test_config,
            context=Context(),
            is_testing=True,
        )
        completion_response = agent.run(prompt=test_sql_completion_request.user_prompt)
        # Customer ID 401 - no need to assess similarity
        assert "401" in completion_response.completion
