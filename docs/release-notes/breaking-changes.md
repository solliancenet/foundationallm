# History of breaking changes

> [!NOTE]
> This section is for changes that are not yet released but will affect future releases.

## Breaking changes that will affect future releases

### Starting with 0.8.0

Core API changes:

1. All Core API endpoints have been moved to the `/instances/{instanceId}` path. For example, the `/status` endpoint is now `/instances/{instanceId}/status`.
2. The `/orchestration/*` endpoints have been moved to `/instances/{instanceId}/completions/*`.
   1. The previous `/orchestration/completions` endpoint is now `/instances/{instanceId}/completions`.
3. The `/sessions/{sessionId}/completion` endpoint has been moved to `/instances/{instanceId}/completions`. Instead of having the `sessionId` as a path parameter, it is now in the request body as part of the `CompletionRequest` payload.
4. `/sessions/{sessionId}/summarize-name` has been removed. In the future, the `/completions` endpoint will be used to generate summaries.
5. `OrchestrationRequest` and `CompletionRequest` have combined into a single `CompletionRequest` object.
6. `DirectionCompletionRequest` has been removed. Use `CompletionRequest` instead.
7. `Status` controllers `\status` action in the .NET API projects return value has renamed the `Instance` property to `InstanceName`.
8. The `CompletionController.cs` file under `dotnet/CoreApi/controllers` has introduced the `Async-Completions` endpoint to handle asynchronous completions.
9. With the introduction of `Async-Completions`, long running operations can now report on completion status based on `Pending`, `InProgress`, `Completed` and `Failed` states.
10. Vectorization Embedding Profile introduces a required field `EmbeddingAIModelObjectId` property (serializes to `embedding_ai_model_object_id`).
11. Vectorization Indexing Profile introduces a required field `IndexingAPIEndpointConfigurationObjectId` property (serializes to `indexing_api_endpoint_configuration_object_id`).

Gatekeeper API changes:
1. All Gatekeeper API endpoints have been moved to the `/instances/{instanceId}` path. For example, the `/status` endpoint is now `/instances/{instanceId}/status`.
2. The `/orchestration/*` endpoints have been moved to `/instances/{instanceId}/completions/*`.

Orchestration API changes:
1. All Gatekeeper API endpoints have been moved to the `/instances/{instanceId}` path. For example, the `/status` endpoint is now `/instances/{instanceId}/status`.
2. The `/orchestration/*` endpoints have been moved to `/instances/{instanceId}/completions/*`.
=======
#### New APIs

**Gateway Adapter API** - requires the following configuration settings:

- `FoundationaLLM:APIs:GatewayAdapterAPI:APIUrl`
- `FoundationaLLM:APIs:GatewayAdapterAPI:APIKey` (mapped to the `foundationallm-apis-gatewayadapterapi-apikey` secret)
- `FoundationaLLM:APIs:GatewayAdapterAPI:APIAppInsightsConnectionString` (mapped to the `foundationallm-app-insights-connection-string` secret)
- 
**State API** - requires the following configuration settings:

- `FoundationaLLM:APIs:StateAPI:APIUrl`
- `FoundationaLLM:APIs:StateAPI:APIKey` (mapped to the `foundationallm-apis-stateapi-apikey` secret)
- `FoundationaLLM:APIs:StateAPI:APIAppInsightsConnectionString` (mapped to the `foundationallm-app-insights-connection-string` secret)

> [!NOTE]
> These new APIs will be converted to use the new `APIEndpoint` artifacts.

#### Changes in app registration names

API Name | Entra ID app registration name | Application ID URI | Scope name
--- | --- | --- | ---
Core API | `FoundationaLLM-Core-API` | `api://FoundationaLLM-Core` | `Data.Read`
Management API | `FoundationaLLM-Management-API` | `api://FoundationaLLM-Management` | `Data.Manage`
Authorization API | `FoundationaLLM-Authorization-API` | `api://FoundationaLLM-Authorization` | `Authorization.Manage`
User Portal | `FoundationaLLM-Core-Portal` | `api://FoundationaLLM-Core-Portal` | N/A
Management Portal | `FoundationaLLM-Management-Portal` | `api://FoundationaLLM-Management-Portal` | N/A

#### Changes in app configuration settings

The `FoundationaLLM:APIs` and `FoundationaLLM:ExternalAPIs` configuration namespaces have been replaced with the `FoundationaLLM:APIEndpoints` configuration namespace.

> [!IMPORTANT]
> All existing API registrations need to be updated to reflect these changes. The only two settings that will exist under `FoundationaLLM:APIEndpoints` are `APIKey` (for those API enpoints which use API key authentication) and `AppInsightsConnectionString`, all the other settings are now part of the `APIEndpoint` artifact managed by the `FoundationaLLM.Configuration` resource provider.
> This is an example for `CoreAPI`:
> - `FoundationaLLM:APIEndpoints:CoreAPI:APIKey`
> - `FoundationaLLM:APIEndpoints:CoreAPI:AppInsightsConnectionString`

The `FoundationaLLM:AzureAIStudio` configuration namespace expects an `APIEndpointConfigurationName` property instead of `BaseUrl`.

### Pre-0.8.0

1. Vectorization resource stores use a unique collection name, `Resources`. They also add a new top-level property named `DefaultResourceName`.
2. The items in the `index_references` collection have a property incorrectly named `type` which was renamed to `index_entry_id`.
3. New gateway API, requires the following app configurations:
   - `FoundationaLLM:APIs:GatewayAPI:APIUrl`
   - `FoundationaLLM:APIs:GatewayAPI:APIKey` (with secret `foundationallm-apis-gatewayapi-apikey`)
   - `FoundationaLLM:APIs:GatewayAPI:AppInsightsConnectionString` (with secret `foundationallm-app-insights-connection-string`)
   - `FoundationaLLM:Gateway:AzureOpenAIAccounts`
4. The `AgentFactory` and `AgentFactoryAPI` classes have been renamed to `Orchestration` and `OrchestrationAPI`, respectively. The following App Config settings need to be replaced in existing environments:

    - `FoundationaLLM:APIs:AgentFactoryAPI:APIKey` -> `FoundationaLLM:APIs:OrchestrationAPI:APIKey`
    - `FoundationaLLM:APIs:AgentFactoryAPI:APIUrl` -> `FoundationaLLM:APIs:OrchestrationAPI:APIUrl`
    - `FoundationaLLM:APIs:AgentFactoryAPI:AppInsightsConnectionString` -> `FoundationaLLM:APIs:OrchestrationAPI:AppInsightsConnectionString`
    - `FoundationaLLM:Events:AzureEventGridEventService:Profiles:AgentFactoryAPI` -> `FoundationaLLM:Events:AzureEventGridEventService:Profiles:OrchestrationAPI`
    - `FoundationaLLM:APIs:AgentFactoryAPI:ForceHttpsRedirection` -? `FoundationaLLM:APIs:OrchestrationAPI:ForceHttpsRedirection`

5. The following Key Vault secrets need to be replaced in existing environments:

    - `foundationallm-apis-agentfactoryapi-apikey` -> `foundationallm-apis-orchestrationapi-apikey`

    There is an upgrade script available that migrates these settings and secrets to their new names.

6. The following App Config settings are no longer needed:

    - `FoundationaLLM:Vectorization:Queues:Embed:ConnectionString`
    - `FoundationaLLM:Vectorization:Queues:Extract:ConnectionString`
    - `FoundationaLLM:Vectorization:Queues:Index:ConnectionString`
    - `FoundationaLLM:Vectorization:Queues:Partition:ConnectionString`

7. The following Key Vault secret is no longer needed:

    - `foundationallm-vectorization-queues-connectionstring`

8. The following App Config settings need to be added as key-values:

   - `FoundationaLLM:Vectorization:Queues:Embed:AccountName` (set to the name of the storage account that contains the vectorization queues - e.g., `stejahszxcubrpi`)
   - `FoundationaLLM:Vectorization:Queues:Extract:AccountName` (set to the name of the storage account that contains the vectorization queues - e.g., `stejahszxcubrpi`)
   - `FoundationaLLM:Vectorization:Queues:Index:AccountName` (set to the name of the storage account that contains the vectorization queues - e.g., `stejahszxcubrpi`)
   - `FoundationaLLM:Vectorization:Queues:Partition:AccountName` (set to the name of the storage account that contains the vectorization queues - e.g., `stejahszxcubrpi`)

9. The value for the App Config setting `FoundationaLLM:Events:AzureEventGridEventService:Profiles:OrchestrationAPI` should be set in the following format:

    ```json
    {
        "EventProcessingCycleSeconds": 20,
        "Topics": [
            {
                "Name": "storage",
                "SubscriptionPrefix": "orch",
                "EventTypeProfiles": [
                    {
                        "EventType": "Microsoft.Storage.BlobCreated",
                        "EventSets": [
                            {
                                "Namespace": "ResourceProvider.FoundationaLLM.Agent",
                                "Source": "/subscriptions/0a03d4f9-c6e4-4ee1-87fb-e2005d2c213d/resourceGroups/rg-fllm-aca-050/providers/Microsoft.Storage/storageAccounts/stejahszxcubrpi",
                                "SubjectPrefix": "/blobServices/default/containers/resource-provider/blobs/FoundationaLLM.Agent"
                            },
                            {
                                "Namespace": "ResourceProvider.FoundationaLLM.Vectorization",
                                "Source": "/subscriptions/0a03d4f9-c6e4-4ee1-87fb-e2005d2c213d/resourceGroups/rg-fllm-aca-050/providers/Microsoft.Storage/storageAccounts/stejahszxcubrpi",
                                "SubjectPrefix": "/blobServices/default/containers/resource-provider/blobs/FoundationaLLM.Vectorization"
                            },
                            {
                                "Namespace": "ResourceProvider.FoundationaLLM.Prompt",
                                "Source": "/subscriptions/0a03d4f9-c6e4-4ee1-87fb-e2005d2c213d/resourceGroups/rg-fllm-aca-050/providers/Microsoft.Storage/storageAccounts/stejahszxcubrpi",
                                "SubjectPrefix": "/blobServices/default/containers/resource-provider/blobs/FoundationaLLM.Prompt"
                            }
                        ]
                    }
                ]
            }
        ]
    }
    ```

10. Vectorization text embedding profiles require only two items in the `configuration_references` section: `DeploymentName` and `Endpoint`. Optionally, a `deployment_name` entry can be specified in the `settings` section to override the default value in `configuration_references.Endpoint`. Here is an example of the updated format for a text embedding profile:

    ```json
    {
        "type": "text-embedding-profile",
        "name": "AzureOpenAI_Embedding_BaselineGlobalMacro",
        "object_id": "/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/AzureOpenAI_Embedding_BaselineGlobalMacro",
        "display_name": null,
        "description": null,
        "text_embedding": "SemanticKernelTextEmbedding",
        "settings": {
            "deployment_name": "embeddings-3-large"
        },
        "configuration_references": {
            "DeploymentName": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:DeploymentName",
            "Endpoint": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:Endpoint"
        },
        "created_on": "0001-01-01T00:00:00+00:00",
        "updated_on": "0001-01-01T00:00:00+00:00",
        "created_by": null,
        "updated_by": null,
        "deleted": false
    }
    ```

11. External orchestration APIs must be configured using the `FoundationaLLM:ExternalAPIs` configuration namespace. For example, the `BaselineTradingGlobalMacro` external API has the following configurations:
    - `FoundationaLLM:ExternalAPIs:BaselineTradingGlobalMacro:APIUrl`
    - `FoundationaLLM:ExternalAPIs:BaselineTradingGlobalMacro:APIKey`

> [!NOTE]
> These entries do not need to be created as part of the deployment process.

12. App Config key namespace that was previously `FoundationaLLM:Vectorization:ContentSources:*` has been moved to `FoundationaLLM:DataSources:*`. All existing keys need to be moved to the new namespace.
13. New app config entries required:

    - `FoundationaLLM:Attachment:ResourceProviderService:Storage:AuthenticationType`
    - `FoundationaLLM:Attachment:ResourceProviderService:Storage:AccountName`

14. App Config key namespace that was previously `FoundationaLLM:Vectorization:ContentSources:*` has been moved to `FoundationaLLM:DataSources:*`. All existing keys need to be moved to the new namespace.

15. The following App Config setting needs to be added/updated as key-values:

   - Add `FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableAzureContentSafetyPromptShield`
   - Add `FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableLakeraGuard`
   - Add `FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableEnkryptGuardrails`
   - Rename `FoundationaLLM:AzureContentSafety:APIKey` in `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:APIKey`
   - Rename `FoundationaLLM:AzureContentSafety:APIUrl` in `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:APIUrl`
   - Rename `FoundationaLLM:AzureContentSafety:HateSeverity` in `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:HateSeverity`
   - Rename `FoundationaLLM:AzureContentSafety:SelfHarmSeverity` in `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:SelfHarmSeverity`
   - Rename `FoundationaLLM:AzureContentSafety:SexualSeverity` in `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:SexualSeverity`
   - Rename `FoundationaLLM:AzureContentSafety:ViolenceSeverity` in `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:ViolenceSeverity`
   - Add `FoundationaLLM:APIs:Gatekeeper:LakeraGuard:APIKey`
   - Add `FoundationaLLM:APIs:Gatekeeper:LakeraGuard:APIUrl`
   - Add `FoundationaLLM:APIs:Gatekeeper:EnkryptGuardrails:APIKey`
   - Add `FoundationaLLM:APIs:Gatekeeper:EnkryptGuardrails:APIUrl`

16. The following Key Vault secret is needed:

    - `lakera-guard-api-key`
    - `enkrypt-guardrails-apikey`



