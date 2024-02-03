# Configuration for deployment

## Configuration settings used by the FoundationaLLM platform

Name | Default | Type | Area | Used by | Description
--- | --- | --- | --- | --- | ---
`foundationallm-core-api-url` |  | Environment variable | User Portal | `ChatThread.vue`, `index.vue` | The URL of the FoundationaLLM Core API.
`foundationallm-core-api-keyvault-name` |  | Environment variable | Core API |  | The name of the Azure Key Vault used by the Core API.
`foundationallm-core-api-entra-instance` | `https://login.microsoftonline.com/` | Environment variable | Core API |  | The Entra instance used by the Core API.
`foundationallm-core-api-entra-tenant-id` | View the [Entra setup document](./authentication/core-authentication-setup-entra.md) for instructions | Environment variable | Core API |  | The Entra tenant ID used by the Core API.
`foundationallm-core-api-entra-client-id` |  | Environment variable | Core API |  | The Entra client ID used by the Core API.
`foundationallm-core-api-entra-client-secret-name` |  | Key Vault secret| Core API |  | Name of the Entra client secret used by the Core API.
`foundationallm-core-api-entra-callback-path` | `/signin-oidc` | Environment variable | Core API |  | The Entra callback path used by the Core API.
`foundationallm-core-api-entra-scopes` |  | Environment variable | Core API |  | The Entra scopes used by the Core API.
`foundationallm-core-api-gatekeeper-api-url` |  | Environment variable | Core API |  | The URL of the Gatekeeper API used by the Core API.
`foundationallm-gatekeeper-api-keyvault-name` |  | Environment variable | Gatekeeper API | | The name of the Azure Key Vault used by the Gatekeeper API.
`foundationallm-gatekeeper-api-key` |  | Key Vault secret| Gatekeeper API |  | The first Gatekeeper API key.
`foundationallm-gatekeeper-api-agentfactory-api-url` |  | Environment variable | Gatekeeper API |  | The URL of the Agent Factory API used by the Gatekeeper API.
`foundationallm-agentfactory-api-keyvault-name` |  | Environment variable | Agent Factory API |  | The name of the Azure Key Vault used by the Agent Factory API.
`foundationallm-agentfactory-api-key` |  | Key Vault secret| Agent Factory API |  | The first Agent Factory API key.
`foundationallm-agentfactory-api-agenthub-api-url` |  | Environment variable | Agent Factory API |  | The URL of the Agent Hub API used by the Agent Factory API.
`foundationallm-agentfactory-api-prompthub-api-url` |  | Environment variable | Agent Factory API |  | The URL of the Prompt Hub API used by the Agent Factory API.
`foundationallm-agentfactory-api-datasourcehub-api-url` |  | Environment variable | Agent Factory API |  | The URL of the Data Source Hub API used by the Agent Factory API.
`foundationallm-agentfactory-api-langchain-api-url` |  | Environment variable | Agent Factory API |  | The URL of the LangChain API used by the Agent Factory API.
`foundationallm-agentfactory-api-semantickernel-api-url` |  | Environment variable | Agent Factory API |  | The URL of the Semantic Kernel API used by the Agent Factory API.
`foundationallm-agenthub-api-key` |  | Key Vault secret| Agent Hub API | `APIKeyValidator` (SDK) | The Agent Hub API key.
`foundationallm-prompthub-api-key` |  | Key Vault secret| Prompt Hub API | `APIKeyValidator` (SDK) | The Prompt Hub API key.
`foundationallm-datasourcehub-api-key` |  | Key Vault secret| Data Source Hub API | `APIKeyValidator` (SDK) | The Data Source Hub API key.
`foundationallm-langchain-api-key` |  | Key Vault secret| LangChain API | `APIKeyValidator` (SDK) | The first LangChain API key.
`foundationallm-langchain-sqldb-testdb-server-name` |  | Environment variable | LangChain API | `SqlDbConfig` (SDK) | The name of the Azure SQL Server used by the LangChain testdb SQL agent.
`foundationallm-langchain-sqldb-testdb-database-name` |  | Environment variable | LangChain API | `SqlDbConfig` (SDK) | The name of the database used by the LangChain testdb SQL agent.
`foundationallm-langchain-sqldb-testdb-username` |  | Environment variable | LangChain API | `SqlDbConfig` (SDK) | The user name used by the LangChain testdb SQL agent.
`foundationallm-langchain-sqldb-testdb-database-password` |  | Key Vault Secret | LangChain API | `SqlDbConfig` (SDK) | The user password used by the LangChain testdb SQL agent.
`foundationallm-azure-openai-api-url` |  | Environment variable | LangChain API | `AzureChatLLM` (SDK) | The URL of the Azure OpenAI API.
`foundationallm-azure-openai-api-completions-deployment` |  | Environment variable | LangChain API | `AzureChatLLM` (SDK) | The name of the completions Azure Open AI deployment used by LangChain API.
`foundationallm-azure-openai-api-completions-model-version` |  | Environment variable | LangChain API | `AzureChatLLM` (SDK) | The version of the completions model used by LangChain API.
`foundationallm-azure-openai-api-version` | | Environment variable | LangChain API | `AzureChatLLM` (SDK) | The version of the Azure Open AI API used by LangChain API.
`foundationallm-azure-openai-api-key` | | Key Vault secret | LangChain API | `AzureChatLLM` (SDK) | The key of the Azure Open AI API used by LangChain API.
`foundationallm-langchain-summary-model-name` | `gpt-35-turbo` | Environment variable | LangChain API | `SummaryAgent` (SDK) | The name of the summary model used by the LangChain summary agent.
`foundationallm-langchain-summary-max-tokens` | `4097` | Environment variable | LangChain API | `SummaryAgent` (SDK) | The maximum number of input tokens used by the LangChain summary agent.
`foundationallm-keyvault-name` | | Environment variable | LangChain API, PythonSDK |`AgentHub`(SDK), `DataSourceHub`(SDK),`PromptHub`(SDK) | | The name of the Azure Key Vault used by the FoundationaLLM platform.
`foundationallm-configuration-allow-environment-variables` |  | Environment variable | PythonSDK | `Configuration`(SDK) | When True checks environment first then key vault, otherwise checks App config (not yet implemented) then key vault 
`foundationallm-storage-connection-string` | | Key Vault secret | PythonSDK | `PromptHubStorageManager`(SDK), `DataSourceHubStorageManager`(SDK), `AgentHubStorageManager`(SDK) | The connection string of the Azure Blob Storage account used by the FoundationaLLM platform.
`foundationallm-prompt-metadata-storage-container` | | Environment variable | PythonSDK | `PromptHubStorageManager`(SDK) | The name of the Azure Blob Storage container where prompt metadata is stored.
`foundationallm-datasource-metadata-storage-container` | | Environment variable | PythonSDK | `DataSourceHubStorageManager`(SDK) | The name of the Azure Blob Storage container where data source metadata is stored.
`foundationallm-agent-metadata-storage-container` | | Environment variable | PythonSDK | `AgentHubStorageManager`(SDK) | The name of the Azure Blob Storage container where agent metadata is stored.

## Temporary configuration settings used by the FoundationaLLM platform

>**NOTE**: These configuration settings are temporary and will be removed in the future.

Name | Type | Area | Used by | Description
--- | --- | --- | --- | ---
`foundationallm-langchain-csv-file-url` | Environment variable | LangChain API | `CSVAgent` (SDK) | The URL (including the SAS token) of the CSV file used by the LangChain CSV agent.
