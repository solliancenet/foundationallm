# SQL Integration Test Setup Instructions

## Introduction

We will be using Microsoft's sample [Wide World Importers (WWI)](https://github.com/microsoft/sql-server-samples/releases/tag/wide-world-importers-v1.0) database to test that LangChain can enrich LLM models with context from SQL.

## Procedure

1. Install Docker
2. Run a Docker container based on the latest Microsoft SQL Server 2022 image. The database will be accessible outside the Docker network.

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Password.1!!" -e "MSSQL_PID=Evaluation" -p 1433:1433  --name wwi -dt --rm mcr.microsoft.com/mssql/server:2022-latest
```

3. Download the WWI database backup to your local host and copy the file to the root of the `wwi` container.

```bash
docker cp WideWorldImporters-Full.bak wwi:/wwi.bak
```

4. Restore the database.

```bash
docker exec wwi /opt/mssql-tools/bin/sqlcmd -U "sa" -P "Password.1!!" -q "RESTORE DATABASE WideWorldImporters FROM DISK = '/wwi.bak' WITH MOVE 'WWI_Primary' TO '/var/opt/mssql/data/WideWorldImporters.mdf', MOVE 'WWI_UserData' TO '/var/opt/mssql/data/WideWorldImporters_userdata.ndf', MOVE 'WWI_Log' TO '/var/opt/mssql/data/WideWorldImporters.ldf', MOVE 'WWI_InMemory_Data_1' TO '/var/opt/mssql/data/WideWorldImporters_InMemory_Data_1'"
```

5. Query the database to verify that data loading completed successfully. Two entries should appear.

```bash
docker exec wwi /opt/mssql-tools/bin/sqlcmd -U "sa" -P "Password.1!!" -d "WideWorldImporters" -q "SELECT TOP(5) * FROM Sales.SpecialDeals;"
```

6. To run these tests, you will need [Microsoft ODBC Driver 18 for SQL Server](https://learn.microsoft.com/en-us/sql/connect/odbc/download-odbc-driver-for-sql-server?view=sql-server-ver16).