from typing import Optional

from langchain_community.tools.sql_database.tool import BaseSQLDatabaseTool
from langchain_core.callbacks import CallbackManagerForToolRun
from langchain_core.pydantic_v1 import Field
from langchain_core.tools import BaseTool

class SecureSQLDatabaseQueryTool(BaseSQLDatabaseTool, BaseTool):
    """Tool for querying a SQL database."""

    name: str = "secure_sql_db_query"
    description: str = """
        Input to this tool is a detailed and correct SQL query, output is a result from the database.
        If the query is not correct, an error message will be returned.
        If an error is returned, rewrite the query, check the query, and try again.
        """

    username: str = Field(exclude=True)
    use_row_level_security: bool = Field(exclude=True)

    def _run(
        self,
        query: str,
        run_manager: Optional[CallbackManagerForToolRun] = None,
    ) -> str:
        """
        Execute the query, conditionally wrapping it in MSSQL row-level security statements.
        Return the results or an error message.
        """
        if (self.use_row_level_security) and (self.db.dialect=='mssql') and (self.username!=''):
            if (query.startswith(f"EXECUTE AS USER = '{self.username}';")) \
                and (query.endswith('REVERT;')):
                query = query
            else:
                query = (
                    f"EXECUTE AS USER = '{self.username}'; "
                    f"{query}; "
                    "REVERT;"
                )
        else:
            query = query

        return self.db.run_no_throw(query)
