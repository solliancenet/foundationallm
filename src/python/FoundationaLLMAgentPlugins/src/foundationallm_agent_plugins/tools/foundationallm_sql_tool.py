"""
Provides an implementation for the FoundationaLLM SQL tool.
"""

# Platform imports
from typing import List, Dict

from datetime import datetime, time, date
import json
import pandas as pd
import pyodbc

#Azure imports

# LangChain imports
from langchain_core.messages import (
    BaseMessage,
    SystemMessage,
    HumanMessage,
    AIMessage
)
from langchain_core.runnables import RunnableConfig

from opentelemetry.trace import SpanKind

# FoundationaLLM imports
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import AgentTool

from foundationallm_agent_plugins.tools import FoundationaLLMIntentToolBase

class FoundationaLLMSQLTool(FoundationaLLMIntentToolBase):
    """
    Provides an implementation for the FoundationaLLM SQL tool.
    """

    def __init__(
        self,
        tool_config: AgentTool,
        objects: Dict,
        user_identity:UserIdentity,
        config: Configuration):
        """ Initializes the FoundationaLLMSQLTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """

        super().__init__(tool_config, objects, user_identity, config)

        self.final_system_prompt = """
        You are an AI assistant that responds to user queries using the provided data context.
        If the context includes tabular data, format the information as a markdown table with clear and human-friendly column headers.
        Do not display or reference the original column names. Always provide concise, informative, and well-structured responses that directly address the user's request.

        Rules when creating your response:
        
        - NEVER expose ID column values in your response, even if they are requested.
        - DO NOT respond that you are not exposing internal database columns or IDs.
        - NEVER expose internal database column names or data types even if they are requested.
        """

        self.__setup_sql_configuration(tool_config, config)

    def _run(
        self,
        *args,
        **kwargs
        ) -> str:

        raise NotImplementedError()

    async def _arun(
        self,
        *args,
        prompt: str = None,
        intents: List[Dict] = None,
        message_history: List[BaseMessage] = None,
        runnable_config: RunnableConfig = None,
        **kwargs,
        ) -> str:

        prompt_tokens = 0
        completion_tokens = 0
        generated_sql_query = ''
        final_response = ''

        # Get the original prompt
        if runnable_config is None:
            original_prompt = prompt
        else:
            original_prompt = runnable_config['configurable']['original_user_prompt']

        messages = [
            SystemMessage(content=self.main_prompt),
            *message_history,
            HumanMessage(content=original_prompt)
        ]

        with self.tracer.start_as_current_span(self.name, kind=SpanKind.INTERNAL):
            try:

                with self.tracer.start_as_current_span(f'{self.name}_initial_llm_call', kind=SpanKind.INTERNAL):
                    
                    response = await self.main_llm.ainvoke(messages, tools=self.tools)

                    completion_tokens += response.usage_metadata['input_tokens']
                    prompt_tokens += response.usage_metadata['output_tokens']
                
                if response.tool_calls \
                    and response.tool_calls[0]['name'] == 'query_azure_sql':

                    tool_call = response.tool_calls[0]

                    with self.tracer.start_as_current_span(f'{self.name}_tool_call', kind=SpanKind.INTERNAL) as tool_call_span:
                        tool_call_span.set_attribute("tool_call_id", tool_call['id'])
                        tool_call_span.set_attribute("tool_call_function", tool_call['name'])

                        function_name = tool_call['name']
                        function_to_call = self.available_sql_functions[function_name]
                        function_args = tool_call['args']
                        if 'query' in function_args:
                            generated_sql_query = function_args['query']

                        function_response = function_to_call(**function_args)

                    final_messages = [
                        SystemMessage(content=self.final_system_prompt),
                        HumanMessage(content=original_prompt),
                        AIMessage(content=function_response)
                    ]

                    with self.tracer.start_as_current_span(f'{self.name}_final_llm_call', kind=SpanKind.INTERNAL):
                    
                        final_llm_response = await self.main_llm.ainvoke(final_messages, tools=None)

                        completion_tokens += final_llm_response.usage_metadata['input_tokens']
                        prompt_tokens += final_llm_response.usage_metadata['output_tokens']
                        final_response = final_llm_response.content

                return final_response, \
                    [
                        self.create_tool_content_artifact(
                            original_prompt,
                            generated_sql_query,
                            prompt_tokens,
                            completion_tokens
                        )
                    ]

            except Exception as e:
                self.logger.error('An error occured in tool %s: %s', self.name, e)
                return self.default_error_message, \
                    [
                        self.create_tool_content_artifact(
                            original_prompt,
                            prompt,
                            prompt_tokens,
                            completion_tokens
                        ),
                        self.create_error_content_artifact(
                            original_prompt,
                            e
                        )
                    ]

    def __setup_sql_configuration(
            self,
            tool_config: AgentTool,
            config: Configuration,
    ):
        
        self.server = tool_config.properties['sql_server']
        self.database = tool_config.properties['sql_database']
        self.username = tool_config.properties['sql_user']
        self.password = config.get_value(tool_config.properties['sql_password_secret_key'])

        self.connection_string = f'Driver={{ODBC Driver 18 for SQL Server}};Server=tcp:{self.server},1433;Database={self.database};Uid={self.username};Pwd={self.password};Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;'

        self.tools = [
            {
                "type": "function",
                "function": {
                    "name": "query_azure_sql",
                    "description": "Execute a SQL query to retrieve information from a database",
                    "parameters": {
                        "type": "object",
                        "properties": {
                            "query": {
                                "type": "string",
                                "description": "The SQL query to execute",
                            },
                        },
                        "required": ["query"],
                    },
                }
            },
            # {
            #     "type": "function",
            #     "function": {
            #         "name": "get_table_schema",
            #         "description": "Get the schema of a table in Azure SQL",
            #         "parameters": {
            #             "type": "object",
            #             "properties": {
            #                 "table_name": {
            #                     "type": "string",
            #                     "description": "The name of the table to get the schema for",
            #                 },
            #             },
            #             "required": ["table_name"],
            #         },
            #     }
            # },
            # {
            #     "type": "function",
            #     "function": {
            #         "name": "get_table_rows",
            #         "description": "Preview the first 5 rows of a table in Azure SQL",
            #         "parameters": {
            #             "type": "object",
            #             "properties": {
            #                 "table_name": {
            #                     "type": "string",
            #                     "description": "The name of the table to get the preview for",
            #                 },
            #             },
            #             "required": ["table_name"],
            #         },
            #     }
            # },
            # {
            #     "type": "function",
            #     "function": {
            #         "name": "get_column_values",
            #         "description": "Get the unique values of a column in a table in Azure SQL, only use this if the main query has a WHERE clause",
            #         "parameters": {
            #             "type": "object",
            #             "properties": {
            #                 "table_name": {
            #                     "type": "string",
            #                     "description": "The name of the table to get the column values for",
            #                 },
            #                 "column_name": {
            #                     "type": "string",
            #                     "description": "The name of the column to get the values for",
            #                 },
            #             },
            #             "required": ["table_name", "column_name"],
            #         },
            #     }
            # },
            ## {
            #     "type": "function",
            #     "function": {
            #         "name": "agent_query_validator",
            #         "description": "Validate a SQL query for common mistakes, always call this before calling query_azure_sql",
            #         "parameters": {
            #             "type": "object",
            #             "properties": {
            #                 "query": {
            #                     "type": "string",
            #                     "description": "The SQL query to validate",
            #                 }
            #             },
            #             "required": ["query"],
            #         },
            #     }
            # }

        ]

        self.available_sql_functions = {
            "query_azure_sql":self.query_azure_sql, 
            # "get_table_schema":self.get_table_schema,
            #"get_table_rows":self.get_table_rows,
            #"get_column_values":self.get_column_values,
            # "agent_query_validator":self.agent_query_validator
        }

    def query_azure_sql(self, query: str) -> str:
        """Run a SQL query on Azure SQL and return results as a pandas DataFrame"""
        print(f"Executing query on Azure SQL: {query}")
        try:
            conn = pyodbc.connect(self.connection_string)
            df = pd.read_sql(query, conn)
            df = self.__convert_datetime_columns_to_string(df)
            return json.dumps(df.to_dict(orient='records'))
        except pyodbc.Error as e:
            self.logger.error("Error occurred while executing a SQL query. Error: %s; Query: %s", e, query)

    def __convert_to_string(self, val):
        if val is None:
            return ''
        if isinstance(val, datetime):
            return val.strftime('%Y-%m-%d %H:%M:%S')
        if isinstance(val, date):
            return val.strftime('%Y-%m-%d')
        if isinstance(val, time):
            return val.strftime('%H:%M:%S')

        return str(val)

    def __convert_datetime_columns_to_string(self, df: pd.DataFrame) -> pd.DataFrame:
        """Convert any datetime and timestamp columns in a DataFrame to strings."""
        try:
            df = df.applymap(self.__convert_to_string)
            return df
        except Exception as e:
            self.logger.error(f"Error occurred while converting SQL datetime columns to string. Error : {e}")