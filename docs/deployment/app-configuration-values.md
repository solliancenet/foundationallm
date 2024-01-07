# Azure App Configuration values

FoundationaLLM uses Azure App Configuration to store configuration values, Key Vault secret references, and feature flags. Doing so helps reduce duplication and provides a convenient way to manage these settings in one place. It also allows you to change the settings without having to redeploy the solution. Since several settings can be shared by multiple projects, we do not specify the project name in the configuration key names.

## Configuration values

| Key | Default Value | Description |
| --- | ------------- | ----------- |
| `FoundationaLLM:AgentHub:AgentMetadata:StorageContainer` | agents |   |
| `FoundationaLLM:AgentHub:StorageManager:BlobStorage:ConnectionString` | Key Vault secret name: `foundationallm-agenthub-storagemanager-blobstorage-connectionstring` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:AgentFactoryAPI:APIKey` | Key Vault secret name: `foundationallm-apis-agentfactoryapi-apikey` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:AgentFactoryAPI:APIUrl` | Enter the URL to the service. |   |
| `FoundationaLLM:APIs:AgentFactoryAPI:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:AgentFactoryAPI:ForceHttpsRedirection` | true | By default, the Agent Factory API forces HTTPS redirection. To override this behavior and allow it to handle HTTP requests, set this value to false. |
| `FoundationaLLM:APIs:AgentHubAPI:APIKey` | Key Vault secret name: `foundationallm-apis-agenthubapi-apikey` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:AgentHubAPI:APIUrl` | Enter the URL to the service. |   |
| `FoundationaLLM:APIs:AgentHubAPI:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:CoreAPI:APIUrl` | Enter the URL to the service. |   |
| `FoundationaLLM:APIs:CoreAPI:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:DataSourceHubAPI:APIKey` | Key Vault secret name: `foundationallm-apis-datasourcehubapi-apikey` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:DataSourceHubAPI:APIUrl` | Enter the URL to the service. |   |
| `FoundationaLLM:APIs:DataSourceHubAPI:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:GatekeeperAPI:APIKey` | Key Vault secret name: `foundationallm-apis-gatekeeperapi-apikey` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:GatekeeperAPI:APIUrl` | Enter the URL to the service. |   |
| `FoundationaLLM:APIs:GatekeeperAPI:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableAzureContentSafety` | true | By default, the Gatekeeper API has Azure Content Safety integration enabled. To disable this feature, set this value to false. |
| `FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableMicrosoftPresidio` | true | By default, the Gatekeeper API has Microsoft Presidio integration enabled. To disable this feature, set this value to false. |
| `FoundationaLLM:APIs:GatekeeperAPI:ForceHttpsRedirection` | true | By default, the Gatekeeper API forces HTTPS redirection. To override this behavior and allow it to handle HTTP requests, set this value to false. |
| `FoundationaLLM:APIs:GatekeeperIntegrationAPI:APIKey` | Key Vault secret name: `foundationallm-apis-gatekeeperintegrationapi-apikey` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:GatekeeperIntegrationAPI:APIUrl` | Enter the URL to the service. |   |
| `FoundationaLLM:APIs:LangChainAPI:APIKey` | Key Vault secret name: `foundationallm-apis-langchainapi-apikey` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:LangChainAPI:APIUrl` | Enter the URL to the service. |   |
| `FoundationaLLM:APIs:LangChainAPI:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:ManagementAPI:APIUrl` | Enter the URL to the service. |   |
| `FoundationaLLM:APIs:ManagementAPI:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:PromptHubAPI:APIKey` | Key Vault secret name: `foundationallm-apis-prompthubapi-apikey` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:PromptHubAPI:APIUrl` | Enter the URL to the service. |   |
| `FoundationaLLM:APIs:PromptHubAPI:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:SemanticKernelAPI:APIKey` | Key Vault secret name: `foundationallm-apis-semantickernelapi-apikey` | This is a Key Vault reference. |
| `FoundationaLLM:APIs:SemanticKernelAPI:APIUrl` | Enter the URL to the service. |   |
| `FoundationaLLM:APIs:SemanticKernelAPI:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | This is a Key Vault reference. |
| `FoundationaLLM:AzureContentSafety:APIKey` | Key Vault secret name: `foundationallm-azurecontentsafety-apikey` | This is a Key Vault reference. |
| `FoundationaLLM:AzureContentSafety:APIUrl` | Enter the URL to the service. |   |
| `FoundationaLLM:AzureContentSafety:HateSeverity` | 2 |   |
| `FoundationaLLM:AzureContentSafety:SelfHarmSeverity` | 2 |   |
| `FoundationaLLM:AzureContentSafety:SexualSeverity` | 2 |   |
| `FoundationaLLM:AzureContentSafety:ViolenceSeverity` | 2 |   |
| `FoundationaLLM:AzureOpenAI:API:Completions:DeploymentName` | completions |   |
| `FoundationaLLM:AzureOpenAI:API:Completions:MaxTokens` | 8096 |   |
| `FoundationaLLM:AzureOpenAI:API:Completions:ModelName` | gpt-35-turbo |   |
| `FoundationaLLM:AzureOpenAI:API:Completions:ModelVersion` | 0301 |   |
| `FoundationaLLM:AzureOpenAI:API:Completions:Temperature` | 0 |   |
| `FoundationaLLM:AzureOpenAI:API:Embeddings:DeploymentName` | embeddings |   |
| `FoundationaLLM:AzureOpenAI:API:Embeddings:MaxTokens` | 8191 |   |
| `FoundationaLLM:AzureOpenAI:API:Embeddings:ModelName` | text-embedding-ada-002 |   |
| `FoundationaLLM:AzureOpenAI:API:Embeddings:Temperature` | 0 |   |
| `FoundationaLLM:AzureOpenAI:API:Endpoint` | Enter the URL to the service. |   |
| `FoundationaLLM:AzureOpenAI:API:Key` | Key Vault secret name: `foundationallm-azureopenai-api-key` | This is a Key Vault reference. |
| `FoundationaLLM:AzureOpenAI:API:Version` | 2023-05-15 |   |
| `FoundationaLLM:BlobStorageMemorySource:BlobStorageConnection` | Key Vault secret name: `foundationallm-blobstoragememorysource-blobstorageconnection` | This is a Key Vault reference. |
| `FoundationaLLM:BlobStorageMemorySource:BlobStorageContainer` | memory-source |   |
| `FoundationaLLM:BlobStorageMemorySource:ConfigFilePath` | BlobMemorySourceConfig.json |   |
| `FoundationaLLM:Branding:AccentColor` | #fff |   |
| `FoundationaLLM:Branding:AllowAgentSelection` | default, SDZWA | These are merely sample agent names. Define one or more agents configured for your environment. **Note:** This value corresponds with the `FoundationaLLM-AllowAgentHint` feature flag. If the feature flag is `true`, then the User Portal UI uses these values to provide agent hints to the Agent Hub in completions-based requests. Otherwise, these values are ignored. |
| `FoundationaLLM:Branding:BackgroundColor` | #fff |   |
| `FoundationaLLM:Branding:CompanyName` | FoundationaLLM |   |
| `FoundationaLLM:Branding:FavIconUrl` | favicon.ico |   |
| `FoundationaLLM:Branding:KioskMode` | false |   |
| `FoundationaLLM:Branding:LogoText` |   |   |
| `FoundationaLLM:Branding:LogoUrl` | foundationallm-logo-white.svg |   |
| `FoundationaLLM:Branding:PageTitle` | FoundationaLLM Chat Copilot |   |
| `FoundationaLLM:Branding:PrimaryColor` | #131833 |   |
| `FoundationaLLM:Branding:PrimaryTextColor` | #fff |   |
| `FoundationaLLM:Branding:SecondaryColor` | #334581 |   |
| `FoundationaLLM:Branding:SecondaryTextColor` | #fff |   |
| `FoundationaLLM:Chat:Entra:CallbackPath` | /signin-oidc |   |
| `FoundationaLLM:Chat:Entra:ClientId` |   |   |
| `FoundationaLLM:Chat:Entra:ClientSecret` | Key Vault secret name: `foundationallm-chat-entra-clientsecret` | This is a Key Vault reference. |
| `FoundationaLLM:Chat:Entra:Instance` | Enter the URL to the service. |   |
| `FoundationaLLM:Chat:Entra:Scopes` | api://FoundationaLLM-Auth/Data.Read |   |
| `FoundationaLLM:Chat:Entra:TenantId` |   |   |
| `FoundationaLLM:CognitiveSearch:EndPoint` | Enter the URL to the service. |   |
| `FoundationaLLM:CognitiveSearch:IndexName` | vector-index |   |
| `FoundationaLLM:CognitiveSearch:Key` | Key Vault secret name: `foundationallm-cognitivesearch-key` | This is a Key Vault reference. |
| `FoundationaLLM:CognitiveSearch:MaxVectorSearchResults` | 10 |   |
| `FoundationaLLM:CognitiveSearchMemorySource:BlobStorageConnection` | Key Vault secret name: `foundationallm-cognitivesearchmemorysource-blobstorageconnection` | This is a Key Vault reference. |
| `FoundationaLLM:CognitiveSearchMemorySource:BlobStorageContainer` | memory-source |   |
| `FoundationaLLM:CognitiveSearchMemorySource:ConfigFilePath` | ACSMemorySourceConfig.json |   |
| `FoundationaLLM:CognitiveSearchMemorySource:EndPoint` | Enter the URL to the service. |   |
| `FoundationaLLM:CognitiveSearchMemorySource:IndexName` | vector-index |   |
| `FoundationaLLM:CognitiveSearchMemorySource:Key` | Key Vault secret name: `foundationallm-cognitivesearchmemorysource-key` | This is a Key Vault reference. |
| `FoundationaLLM:CoreAPI:Entra:CallbackPath` | /signin-oidc |   |
| `FoundationaLLM:CoreAPI:Entra:ClientId` |   |   |
| `FoundationaLLM:CoreAPI:Entra:ClientSecret` | Key Vault secret name: `foundationallm-coreapi-entra-clientsecret` | This is a Key Vault reference. |
| `FoundationaLLM:CoreAPI:Entra:Instance` | Enter the URL to the service. |   |
| `FoundationaLLM:CoreAPI:Entra:Scopes` | Data.Read |   |
| `FoundationaLLM:CoreAPI:Entra:TenantId` |   |   |
| `FoundationaLLM:CoreWorker:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | This is a Key Vault reference. |
| `FoundationaLLM:CosmosDB:ChangeFeedLeaseContainer` | leases |   |
| `FoundationaLLM:CosmosDB:Containers` | Sessions, UserSessions |   |
| `FoundationaLLM:CosmosDB:Database` | database |   |
| `FoundationaLLM:CosmosDB:Endpoint` | Enter the URL to the service. |   |
| `FoundationaLLM:CosmosDB:Key` | Key Vault secret name: `foundationallm-cosmosdb-key` | This is a Key Vault reference. |
| `FoundationaLLM:CosmosDB:MonitoredContainers` | Sessions |   |
| `FoundationaLLM:DataSourceHub:DataSourceMetadata:StorageContainer` | data-sources |   |
| `FoundationaLLM:DataSourceHub:StorageManager:BlobStorage:ConnectionString` | Key Vault secret name: `foundationallm-datasourcehub-storagemanager-blobstorage-connectionstring` | This is a Key Vault reference. |
| `FoundationaLLM:DataSources:AboutFoundationaLLM:BlobStorage:ConnectionString` | Key Vault secret name: `foundationallm-datasourcehub-storagemanager-blobstorage-connectionstring` | This is a Key Vault reference. |
| `FoundationaLLM:DurableSystemPrompt:BlobStorageConnection` | Key Vault secret name: `foundationallm-durablesystemprompt-blobstorageconnection` | This is a Key Vault reference. |
| `FoundationaLLM:DurableSystemPrompt:BlobStorageContainer` | system-prompt |   |
| `FoundationaLLM:LangChain:CSVFile:URL` | Key Vault secret name: `foundationallm-langchain-csvfile-url` | This is a Key Vault reference. |
| `FoundationaLLM:LangChain:SQLDatabase:TestDB:Password` | Key Vault secret name: `foundationallm-langchain-sqldatabase-testdb-password` | This is a Key Vault reference. |
| `FoundationaLLM:LangChain:Summary:MaxTokens` | 4097 |   |
| `FoundationaLLM:LangChain:Summary:ModelName` | gpt-35-turbo |   |
| `FoundationaLLM:LangChainAPI:Key` | Key Vault secret name: `foundationallm-langchainapi-key` | This is a Key Vault reference. |
| `FoundationaLLM:Management:Entra:CallbackPath` | /signin-oidc |   |
| `FoundationaLLM:Management:Entra:ClientId` |   |   |
| `FoundationaLLM:Management:Entra:ClientSecret` | Key Vault secret name: `foundationallm-management-entra-clientsecret` | This is a Key Vault reference. |
| `FoundationaLLM:Management:Entra:Instance` | Enter the URL to the service. |   |
| `FoundationaLLM:Management:Entra:Scopes` | api://FoundationaLLM-Management-Auth/Data.Manage |   |
| `FoundationaLLM:Management:Entra:TenantId` |   |   |
| `FoundationaLLM:ManagementAPI:Entra:ClientId` |   |   |
| `FoundationaLLM:ManagementAPI:Entra:ClientSecret` | Key Vault secret name: `foundationallm-managementapi-entra-clientsecret` | This is a Key Vault reference. |
| `FoundationaLLM:ManagementAPI:Entra:Instance` | Enter the URL to the service. |   |
| `FoundationaLLM:ManagementAPI:Entra:Scopes` | Data.Manage |   |
| `FoundationaLLM:ManagementAPI:Entra:TenantId` |   |   |
| `FoundationaLLM:OpenAI:API:Endpoint` | Enter the URL to the service. |   |
| `FoundationaLLM:OpenAI:API:Key` | Key Vault secret name: `foundationallm-openai-api-key` | This is a Key Vault reference. |
| `FoundationaLLM:OpenAI:API:Temperature` | 0 |   |
| `FoundationaLLM:PromptHub:PromptMetadata:StorageContainer` | system-prompt |   |
| `FoundationaLLM:PromptHub:StorageManager:BlobStorage:ConnectionString` | Key Vault secret name: `foundationallm-prompthub-storagemanager-blobstorage-connectionstring` | This is a Key Vault reference. |
| `FoundationaLLM:Refinement` |   |   |
| `FoundationaLLM:SemanticKernelAPI:OpenAI:Key` | Key Vault secret name: `foundationallm-semantickernelapi-openai-key` | This is a Key Vault reference. |
| `FoundationaLLM:SemanticKernelAPI:OpenAI.ChatCompletionPromptName` | RetailAssistant.Default |   |
| `FoundationaLLM:SemanticKernelAPI:OpenAI.CompletionsDeployment` | completions |   |
| `FoundationaLLM:SemanticKernelAPI:OpenAI.CompletionsDeploymentMaxTokens` | 8096 |   |
| `FoundationaLLM:SemanticKernelAPI:OpenAI.EmbeddingsDeployment` | embeddings |   |
| `FoundationaLLM:SemanticKernelAPI:OpenAI.EmbeddingsDeploymentMaxTokens` | 8191 |   |
| `FoundationaLLM:SemanticKernelAPI:OpenAI.Endpoint` | Enter the URL to the service. |   |
| `FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.CompletionsMaxTokens` | 300 |   |
| `FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.CompletionsMinTokens` | 50 |   |
| `FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MemoryMaxTokens` | 3000 |   |
| `FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MemoryMinTokens` | 1500 |   |
| `FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MessagesMaxTokens` | 3000 |   |
| `FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MessagesMinTokens` | 100 |   |
| `FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.SystemMaxTokens` | 1500 |   |
| `FoundationaLLM:SemanticKernelAPI:OpenAI.ShortSummaryPromptName` | Summarizer.TwoWords |   |
| `FoundationaLLM:Vectorization:WorkerSettings`| `{"RequestManagers": [{ "RequestSourceName": "extract", "MaxHandlerInstances": 1 }], "QueuingEngine": 1 }`  | |

## Feature flags

| Key | Default Value | Description |
| --- | ------------- | ----------- |
| `FoundationaLLM-AllowAgentHint` | `false` | Used for demo purposes. If the feature is enabled, the User Portal UI displays an agent hint selector for a chat session and sends an `X-AGENT-HINT` header with the selected agent name (if applicable) to all HTTP requests to the Core API. This header flows downstream to the Agent Hub, forcing the resolver to use the specified agent. The Agent Hub only uses this header value if this feature flag is enabled, as an added protective measure. |
