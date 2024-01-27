"""
Toolkit for performing anomaly detection.
"""
from typing import List

from langchain.agents import AgentExecutor
from langchain_community.agent_toolkits.base import BaseToolkit
from langchain_core.pydantic_v1 import Field
from langchain_core.tools import BaseTool

from foundationallm.langchain.tools import (
    QueryPandasDataFrameTool,
    TypeConversionTool
)

class AnomalyDetectionToolkit(BaseToolkit):
    """Toolkit for performing anomaly detection."""

    df_agent: AgentExecutor = Field(exclude=True)
    py_agent: AgentExecutor = Field(exclude=True)

    class Config:
        """Configuration for this pydantic object."""
        arbitrary_types_allowed = True

    def get_tools(self) -> List[BaseTool]:
        """Get the tools in the toolkit."""
        query_pandas_dataframe_tool_description = (
            "Input to this tool is a query to describe a feature within the DataFrame."
            "Output is the result of the describe query."
            "Always use this tool to generate statistics before executing a query with product_database."
        )
        query_pandas_dataframe_tool = QueryPandasDataFrameTool(agent=self.df_agent,
                                                               description=query_pandas_dataframe_tool_description)

        type_conversion_tool_description = (
            "Input to this tool is a value of one type that should be converted to another specified type."
            "Output is the value converted to the specified type."
        )
        type_conversion_tool = TypeConversionTool(agent=self.py_agent, description=type_conversion_tool_description)

        return [
            query_pandas_dataframe_tool,
            type_conversion_tool
        ]
