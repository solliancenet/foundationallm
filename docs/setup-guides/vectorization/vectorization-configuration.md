# Configuring vectorization

This section provides details on how to configure the vectorization API and workers in FoundationaLLM.

> [!NOTE]
> These configurations should already be in place if you deployed FoundationaLLM (FLLM) using the recommended deployment scripts.
> The detailes presented here are provided for cases in which you need to troubleshoot or customize the configuration.

## Configuration for Vectorization API

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
> Refer to the [App Configuration values](../../deployment/app-configuration-values.md) page for more information on how to set these and other configuration values.

## Configuration for Vectorization workers

The following table describes the Azure artifacts required for the vectorization pipelines.

| Artifact Name | Description |
| --- | --- |
| `embed` | Azure storage queue used for the embed vectorization pipeline. Can be created on the storage account used for the other queues. |
| `extract` | Azure storage queue used for the extract vectorization pipeline. Can be created on the storage account used for the other queues. |
| `index` | Azure storage queue used for the index vectorization pipeline. Can be created on the storage account used for the other queues. |
| `partition` | Azure storage queue used for the partition vectorization pipeline. Can be created on the storage account used for the other queues. |
| `vectorization-state` | Azure storage container used for the vectorization state service. Can be created on the storage account used for the other queues. |
| `resource-provider`| Azure storage container used for the internal states of the FoundationaLLM resource providers. |
| `resource-provider/FoundationaLLM.Vectorization/vectorization-content-source-profiles.json` | Azure storage blob used for the content sources managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [default vectorization content source profiles](#default-vectorization-content-source-profiles).
| `resource-provider/FoundationaLLM.Vectorization/vectorization-text-partitioning-profiles.json` | Azure storage blob used for the text partitioning profiles managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [default vectorization text partitioning profiles](#default-vectorization-text-partitioning-profiles).
| `resource-provider/FoundationaLLM.Vectorization/vectorization-text-embedding-profiles.json` | Azure storage blob used for the text embedding profiles managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [default vectorization text embedding profiles](#default-vectorization-text-embedding-profiles).
| `resource-provider/FoundationaLLM.Vectorization/vectorization-indexing-profiles.json` | Azure storage blob used for the indexing profiles managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [default vectorization indexing profiles](#default-vectorization-indexing-profiles).

The following table describes the environment variables required for the vectorization pipelines.

| Environment variable | Description |
| --- | --- |
| `FoundationaLLM:AppConfig:ConnectionString` | Connection string to the Azure App Configuration instance. |

The following table describes the required App Configuration parameters for the vectorization pipelines.

| App Configuration Key | Default Value | Description |
| --- | --- | --- |
| `FoundationaLLM:APIs:VectorizationWorker:APIUrl` | | The URL of the vectorization worker API. |
| `FoundationaLLM:APIs:VectorizationWorker:APIKey` | Key Vault secret name: `foundationallm-apis-vectorizationworker-apikey` | The API key of the vectorization worker API. |
| `FoundationaLLM:APIs:VectorizationWorker:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | The connection string to the Application Insights instance used by the vectorization worker API. |
| `FoundationaLLM:Vectorization:VectorizationWorker` | | The settings used by each instance of the vectorization worker service. For more details, see [default vectorization worker settings](#default-vectorization-worker-settings). |
| `FoundationaLLM:Vectorization:Queues:Embed:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-queues-connectionstring` | The connection string to the Azure Storage account used for the embed vectorization queue. |
| `FoundationaLLM:Vectorization:Queues:Extract:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-queues-connectionstring` | The connection string to the Azure Storage account used for the extract vectorization queue. |
| `FoundationaLLM:Vectorization:Queues:Index:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-queues-connectionstring` | The connection string to the Azure Storage account used for the index vectorization queue. |
| `FoundationaLLM:Vectorization:Queues:Partition:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-queues-connectionstring` | The connection string to the Azure Storage account used for the partition vectorization queue. |
| `FoundationaLLM:Vectorization:StateService:Storage:AuthenticationType` | | The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`. |
| `FoundationaLLM:Vectorization:StateService:Storage:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-state-connectionstring` | The connection string to the Azure Storage account used for the vectorization state service. |
| `FoundationaLLM:Vectorization:ResourceProviderService:Storage:AuthenticationType` | | The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`. |
| `FoundationaLLM:Vectorization:ResourceProviderService:Storage:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-resourceprovider-storage-connectionstring` | The connection string to the Azure Storage account used for the vectorization state service. |
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIKey` | Key Vault secret name: `foundationallm-vectorization-semantickerneltextembedding-openai-apikey` | The API key used to connect to the Azure OpenAI service.
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:AuthenticationType` | | The authentication type used to connect to the Azure OpenAI service. Can be one of `AzureIdentity` or `APIKey`.
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:DeploymentName` | | The name of the Azure OpenAI model deployment. The default value is `embeddings`.
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:Endpoint` | | The endpoint of the Azure OpenAI service.
| `FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey` | Key Vault secret name: `foundationallm-vectorization-azureaisearch-apikey` | The API key used to connect to the Azure OpenAI service.
| `FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType` | | The authentication type used to connect to the Azure OpenAI service. Can be one of `AzureIdentity` or `APIKey`.
| `FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint` | | The endpoint of the Azure OpenAI service.

> [!NOTE]
> Refer to the [App Configuration values](../../app-configuration-values.md) page for more information on how to set these and other configuration values.

The following table describes the external content used by the vectorization worker to initialize:

| Uri | Description |
| --- | --- |
| `https://openaipublic.blob.core.windows.net/encodings/cl100k_base.tiktoken` | The public Azure Blob Storage account used to download the OpenAI BPE ranking files. |

> [!NOTE]
> The vectorization worker must be able to open HTTPS connections to the external content listed above.

### Default vectorization worker settings

The default settings for the vectorization worker are stored in the `FoundationaLLM:Vectorization:VectorizationWorker` App Configuration key. The default structure for this key is:

```json
{
    "RequestManagers": [
        {
            "RequestSourceName": "extract",
            "MaxHandlerInstances": 1
        },
        {
            "RequestSourceName": "partition",
            "MaxHandlerInstances": 1
        },
        {
            "RequestSourceName": "embed",
            "MaxHandlerInstances": 1
        },
        {
            "RequestSourceName": "index",
            "MaxHandlerInstances": 1
        }
    ],
    "RequestSources": [
        {
            "Name": "extract",
            "ConnectionConfigurationName": "Extract:ConnectionString",
            "VisibilityTimeoutSeconds": 600
        },
        {
            "Name": "partition",
            "ConnectionConfigurationName": "Partition:ConnectionString",
            "VisibilityTimeoutSeconds": 600
        },
        {
            "Name": "embed",
            "ConnectionConfigurationName": "Embed:ConnectionString",
            "VisibilityTimeoutSeconds": 600
        },
        {
            "Name": "index",
            "ConnectionConfigurationName": "Index:ConnectionString",
            "VisibilityTimeoutSeconds": 600
        }
    ],
    "QueuingEngine": "AzureStorageQueue"
}
```

The following table provides details about the configuration parameters:

| Parameter | Description |
| --- | --- |
| `RequestManagers` | The list of request managers used by the vectorization worker. Each request manager is responsible for managing the execution of vectorization pipelines for a specific vectorization step. The configuration must include all request managers. |
| `RequestManagers.MaxHandlerInstances` | The maximum number of request handlers that process requests for the specified request source. By default, the value is 1. You can change the value to increase the processing capacity of each vectorization worker instance. The value applies to all istances of the vectorization worker. NOTE: It is important to align the value of this setting with the level of compute and memory resources allocated to the individual vectorization worker instances. |
| `RequestSources` | The list of request sources used by the vectorization worker. Each request source is responsible for managing the requests for a specific vectorization step. The configuration must include all request sources. |
| `RequestSources.VisibilityTimeoutSeconds` | In the case of queue-based request sources (the default for the vectorization worker), specifies the time in seconds until a dequeued vectorization step request must be executed. During this timeout, the message will not be visible to other handler instances within the same worker or from other worker instances. If the handler fails to process the vectorization step request successfully and remove it from the queue within the specified timeout, the message will become visibile again. The default value is 600 seconds and should not be changed.|

### Default vectorization content source profiles

Default structure for the `vectorization-content-source-profiles.json` file:

```json
{
    "Profiles": [
        {
            "Name": "DefaultAzureDataLake",
            "Type": "AzureDataLake",
            "ObjectId": "/instances/<instance_id>/providers/FoundationaLLM.Vectorization/contentsourceprofiles/DefaultAzureDataLake",
            "Settings": {},
            "ConfigurationReferences": {
                "AuthenticationType": "FoundationaLLM:Vectorization:ContentSources:DefaultAzureDataLake:AuthenticationType",
                "ConnectionString": "FoundationaLLM:Vectorization:ContentSources:DefaultAzureDataLake:ConnectionString"
            }
        }
    ]
}
```

By default, FLLM includes one content source profile named `DefaultAzureDataLake`. You can add content source profiles to this file to configure the content sources used by the vectorization pipelines. For more details, see [Managing vectorization profiles](vectorization-profiles.md).

Currently, the following content source types are supported:

- `AzureDataLake` - uses an Azure Data Lake storage account as the content source (see [`AzureDataLakeContentSource`](./vectorization-profiles.md#azuredatalake)).
- `SharePointOnline` - uses a SharePoint Online site as the content source (see [`SharePointOnlineContentSource`](./vectorization-profiles.md#sharepointonline)).
- `AzureSQLDatabase` - uses an Azure SQL database as the content source (see [`AzureSQLDatabaseContentSource`](./vectorization-profiles.md#azuresqldatabase)).

### Default vectorization text partitioning profiles

Default structure for the `vectorization-text-partitioning-profiles.json` file:

```json
{
    "Profiles": [
        {
            "Name": "DefaultTokenTextPartition",
            "ObjectId": "/instances/<instance_id>/providers/FoundationaLLM.Vectorization/textpartitioningprofiles/DefaultTokenTextPartition",
            "TextSplitter": "TokenTextSplitter",
            "Settings": {
                "Tokenizer": "MicrosoftBPETokenizer",
                "TokenizerEncoder": "cl100k_base",
                "ChunkSizeTokens": "2000",
                "OverlapSizeTokens": "200"
            }
        }
    ]
}
```

By default, FLLM includes one text partitioning profile named `DefaultTokenTextPartition` which uses the `TokenTextSplitter` text splitter. You can add text partitioning profiles to this file to configure the text partitioning used by the vectorization pipelines. For more details, see [Managing vectorization profiles](vectorization-profiles.md).

Currently, the following text splitters are supported:

- `TokenTextSplitter` - splits the text into chunks based on the number of tokens (see [`TextTokenSplitter`](./vectorization-profiles.md#texttokensplitter)).

### Default vectorization text embedding profiles

Default structure for the `vectorization-text-embedding-profiles.json` file:

```json
{
    "Profiles": [
        {
            "Name": "AzureOpenAI_Embedding",
            "ObjectId": "/instances/<instance_id>/providers/FoundationaLLM.Vectorization/textembeddingprofiles/AzureOpenAI_Embedding",
            "TextEmbedding": "SemanticKernelTextEmbedding",
            "Settings": {},
            "ConfigurationReferences": {
                "APIKey": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIKey",
                "APIVersion": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APVersion",
                "AuthenticationType": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:AuthenticationType",
                "DeploymentName": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:DeploymentName",
                "Endpoint": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:Endpoint"
            }
        }
    ]
}
```

By default, FLLM includes one text embedding profile named `AzureOpenAI_Embedding` which uses the `SemanticKernelTextEmbedding` text embedder. You can add text embedding profiles to this file to configure the text embedding used by the vectorization pipelines. For more details, see [Managing vectorization profiles](vectorization-profiles.md).

Currently, the following text embedders are supported:

- `SemanticKernelTextEmbedding` - embeds the text using Semantic Kernel to call into the default FLLM Azure OpenAI embedding model (see [`SemanticKernelTextEmbedding`](./vectorization-profiles.md#semantickerneltextembedding)).


### Default vectorization indexing profiles

Default structure for the `vectorization-indexing-profiles.json` file:

```json
{
    "Profiles": [
        {
            "Name": "AzureAISearch_Default_001",
            "ObjectId": "/instances/<instance_id>/providers/FoundationaLLM.Vectorization/indexingprofiles/AzureAISearch_Default_001",
            "Indexer": "AzureAISearchIndexer",
            "Settings": {
                "IndexName": "fllm-default-001",
                "TopN": "3",
                "Filters": "",
                "EmbeddingFieldName": "Embedding",
                "TextFieldName": "Text"
            },
            "ConfigurationReferences": {
                "APIKey": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey",
                "QueryAPIKey": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:QueryAPIKey",
                "AuthenticationType": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType",
                "Endpoint": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint"
            }
        }
    ]
}
```

By default, FLLM includes one indexing profile named `AzureAISearch_Default_001` which uses the `AzureAISearchIndexer` indexer. You can add indexing profiles to this file to configure the indexing used by the vectorization pipelines. For more details, see [Managing vectorization profiles](vectorization-profiles.md).

Currently, the following indexers are supported:

- `AzureAISearchIndexer` - indexes the vectors into an Azure AI Search index (see [`AzureAISearchIndexer`](./vectorization-profiles.md#azureaisearchindexer)).
