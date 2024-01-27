# Vectorization API

## Configuration

The following table describes the Azure artifacts required for the vectorization pipelines.

| Artifact name | Description |
| --- | --- |
| `vectorization-input` | Azure storage container used by default to store documents to be picked up by the vectorization pipeline. Must be created on a Data Lake storage account (with the hierarchical namespace enabled). |

The following table describes the environment variables required for the vectorization pipelines.

Environment variable | Description
--- | ---
`FoundationaLLM:AppConfig:ConnectionString` | Connection string to the Azure App Configuration instance.

The following table describes the required configuration parameters for the vectorization pipelines.

| App Configuration Key | Default Value | Description |
| --- | --- | --- |
| `FoundationaLLM:APIs:VectorizationAPI:APIUrl` | | The URL of the vectorization API. |
| `FoundationaLLM:APIs:VectorizationAPI:APIKey` | Key Vault secret name: `foundationallm-apis-vectorizationapi-apikey` | The API key of the vectorization API. |
| `FoundationaLLM:APIs:VectorizationAPI:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | The connection string to the Application Insights instance used by the vectorization API. |

> [!NOTE]
> Refer to the [App Configuration values](../../app-configuration-values.md) page for more information on how to set these and other configuration values.
