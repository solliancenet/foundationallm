from typing import Optional

from langchain.agents import AgentExecutor
from langchain_core.callbacks import CallbackManagerForToolRun
from langchain_core.pydantic_v1 import BaseModel, Field
from langchain_core.tools import BaseTool

class BasePandasDataFrameTool(BaseModel):
    """Base tool for interacting with a Pandas DataFrame."""

    #df: DataFrame = Field(exclude = True)
    agent: AgentExecutor = Field(exclude = True)

    # Override BaseTool.Config to appease mypy
    # See https://github.com/pydantic/pydantic/issues/4173
    class Config(BaseTool.Config):
        """Configuration for this pydantic object."""

        arbitrary_types_allowed = True
        extra = 'forbid'


class QueryPandasDataFrameTool(BasePandasDataFrameTool, BaseTool):
    """Tool for performing a describe operation on a Pandas DataFrame."""

    name: str = 'query_pandas_dataframe'
    description: str = """
    Input to this tool is a query asking to describe various features of the dataframe.
    Output is the result from the dataframe.
    """

    def _run(
        self,
        query: str,
        run_manager: Optional[CallbackManagerForToolRun] = None
    ) -> str:
        """Execute the query, return the results or an error message."""
        return self.agent.run(query)

# class DescribeQueryBuilderTool(BasePandasDataFrameTool, BaseTool):
#     """Tool for generating a describe query for a Pandas DataFrame."""

#     name: str = 'build_dataframe_describe_query'
#     description: str = """
#     Input to this tool is a feature of the dataframe to describe.
#     Output is a query to describe the particular feature of the dataframe and explain 
#       how results should be returned.
#     """

#     @root_validator(pre=True)
#     def initialize_llm_chain()

#     def _run(

#     )

class TypeConversionTool(BaseTool):
    """Tool for converting data types to the same type so records can be compared."""

    agent: AgentExecutor = Field(exclude=True)

    name: str = 'convert_type'
    description: str = """
    Input to this tool is a value of one type that should be converted to another specified type.
    """

    def _run(
        self,
        query: str,
        run_manager: Optional[CallbackManagerForToolRun] = None
    ) -> str:
        """Execute the query, return the results or an error message."""
        return self.agent.run(query)
