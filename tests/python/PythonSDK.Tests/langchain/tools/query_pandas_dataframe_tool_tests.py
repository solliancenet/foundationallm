import pytest
from foundationallm.config import Configuration
from foundationallm.models.language_models import (
    LanguageModelType,
    LanguageModel,
    LanguageModelProvider,
)
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.tools import QueryPandasDataFrameTool
from langchain.agents import create_pandas_dataframe_agent
import pandas as pd


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
def test_pd_df():
    return pd.DataFrame(
        [
            {
                "Product ID": 1,
                "Buyer ID": 1,
                "Product Name": "Cleaning Agent #1",
                "Number Purchased": 375,
            },
            {
                "Product ID": 2,
                "Buyer ID": 1,
                "Product Name": "Cleaning Agent #2",
                "Number Purchased": 425,
            },
            {
                "Product ID": 1,
                "Buyer ID": 3,
                "Product Name": "Cleaning Agent #1",
                "Number Purchased": 200,
            },
            {
                "Product ID": 3,
                "Buyer ID": 2,
                "Product Name": "Cleaning Agent #3",
                "Number Purchased": 450,
            },
            {
                "Product ID": 4,
                "Buyer ID": 3,
                "Product Name": "Sponge",
                "Number Purchased": 100,
            },
        ]
    )


@pytest.fixture
def pandas_dataframe_agent(test_tool_llm, test_pd_df):
    return create_pandas_dataframe_agent(
        llm=test_tool_llm,
        df=test_pd_df,
        prefix="You are a helpful agent designed to generate comma-separated lists about data in a Pandas DataFrame named \"df\". ONLY return a comma-separated list, and nothing more.",
        max_iterations=20,
        verbose=True,
        handle_parsing_errors="Get the final answer from the output as a comma-separated list without quotes, verify it is correct, and return that as the response!",
    )


@pytest.fixture
def query_pandas_dataframe_tool(pandas_dataframe_agent):
    return QueryPandasDataFrameTool(agent=pandas_dataframe_agent)


class QueryPandasDataFrameToolTests:
    """
    QueryPandasDataFrameToolTests is responsible for testing QueryPandasDataFrameTool's
        ability to answer reasonable questions about Pandas DataFrames.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    def test_pandas_dataframe_tool(self, query_pandas_dataframe_tool):
        completion_response = query_pandas_dataframe_tool._run(
            "What are the names of the two most popular products ordered by the total number of units purchased?"
        )
        products = [
            product.strip()
            for product in completion_response.strip("'").strip('"').split(",")
        ]
        assert "Cleaning Agent #1" in products
        assert len(products) == 2
        assert "Cleaning Agent #3" in products