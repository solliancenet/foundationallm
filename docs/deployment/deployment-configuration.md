# Configuration for deployment

## Configuration settings used by the FoundationaLLM platform

Name | Default | Type | Area | Used by | Description
--- | --- | --- | --- | --- | ---
`foundationallm-core-api-url` |  | Environment variable | User Portal | `ChatThread.vue`, `index.vue` | The URL of the FoundationaLLM Core API.
`foundationallm-core-api-keyvault-name` |  | Environment variable | Core API |  | The name of the Azure Key Vault used by the Core API.
`foundationallm-core-api-entra-instance` | `https://login.microsoftonline.com/` | Environment variable | Core API |  | The Entra instance used by the Core API.
`foundationallm-core-api-entra-tenant-id` | | Environment variable | Core API |  | The Entra tenant ID used by the Core API.
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
`foundationallm-agenthub-api-keyvault-name` |  | Environment variable | Agent Hub API | `Configuration` (SDK) | The name of the Azure Key Vault used by the Agent Hub API.
`foundationallm-agenthub-api-key` |  | Key Vault secret| Agent Hub API | `APIKeyValidator` (SDK) | The first Agent Hub API key.
`foundationallm-prompthub-api-keyvault-name` |  | Environment variable | Prompt Hub API | `Configuration` (SDK) | The name of the Azure Key Vault used by the Prompt Hub API.
`foundationallm-prompthub-api-key` |  | Key Vault secret| Prompt Hub API | `APIKeyValidator` (SDK) | The first Prompt Hub API key.
`foundationallm-datasourcehub-api-keyvault-name` |  | Environment variable | Data Source Hub API | `Configuration` (SDK) | The name of the Azure Key Vault used by the Data Source Hub API.
`foundationallm-datasourcehub-api-key` |  | Key Vault secret| Data Source Hub API | `APIKeyValidator` (SDK) | The first Data Source Hub API key.
`foundationallm-langchain-api-keyvault-name` |  | Environment variable | LangChain API | `Configuration` (SDK) | The name of the Azure Key Vault used by the LangChain API.
`foundationallm-langchain-api-key` |  | Key Vault secret| LangChain API | `APIKeyValidator` (SDK) | The first LangChain API key.
`foundationallm-langchain-sqldb-cocorahs-server-name` |  | Environment variable | LangChain API | `SqlDbConfig` (SDK) | The name of the Azure SQL Server used by the LangChain Cocorahs SQL agent.
`foundationallm-langchain-sqldb-cocorahs-database-name` |  | Environment variable | LangChain API | `SqlDbConfig` (SDK) | The name of the database used by the LangChain Cocorahs SQL agent.
`foundationallm-langchain-sqldb-cocorahs-user-name` |  | Environment variable | LangChain API | `SqlDbConfig` (SDK) | The user name used by the LangChain Cocorahs SQL agent.
`foundationallm-langchain-sqldb-cocorahs-user-password` |  | Key Vault Secret | LangChain API | `SqlDbConfig` (SDK) | The user password used by the LangChain Cocorahs SQL agent.
`foundationallm-azure-openai-url` |  | Environment variable | LangChain API | `AzureChatLLM` (SDK) | The URL of the Azure OpenAI API.
`foundationallm-azure-openai-completions-deployment` |  | Environment variable | LangChain API | `AzureChatLLM` (SDK) | The name of the completions Azure Open AI deployment used by LangChain API.
`foundationallm-langchain-summary-model-name` | `gpt-35-turbo` | Environment variable | LangChain API | `SummaryAgent` (SDK) | The name of the summary model used by the LangChain summary agent.
`foundationallm-langchain-summary-max-tokens` | `4097` | Environment variable | LangChain API | `SummaryAgent` (SDK) | The maximum number of input tokens used by the LangChain summary agent.

## Temporary configuration settings used by the FoundationaLLM platform

>**NOTE**: These configuration settings are temporary and will be removed in the future.

Name | Type | Area | Used by | Description
--- | --- | --- | --- | ---
`foundationallm-langchain-csv-file-url` | Environment variable | LangChain API | `CSVAgent` (SDK) | The URL (including the SAS token) of the CSV file used by the LangChain CSV agent.