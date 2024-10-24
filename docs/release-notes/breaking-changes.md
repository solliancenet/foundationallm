# History of breaking changes

> [!NOTE]
> This section is for changes that are not yet released but will affect future releases.

## Starting with 0.8.4

### Resource provider changes

**FoundationaLLM.AzureOpenAI**

When upgrading an existing FoundationaLLM instance, the items from the `resource-provider/FoundationaLLM.AzureOpenAI` directory in storage account must be updated to use the new JSON format:

*Assistant user context*
```json
{
    "name": "example_solliance_net-assistant-00000000-0000-0000-0000-000000000000",
    "object_id": "/instances/00000000-0000-0000-0000-000000000000/providers/FoundationaLLM.AzureOpenAI/assistantUserContexts/example_solliance_net-assistant-00000000-0000-0000-0000-000000000000",
    "display_name": null,
    "description": null,
    "cost_center": null,
    "user_principal_name": "example@solliance.net",
    "agent-assistants": {
        "/instances/00000000-0000-0000-0000-000000000000/providers/FoundationaLLM.Agent/agents/GPT-4o": {
            "endpoint": "https://example.openai.azure.com/",
            "model_name": "completions-gpt-4o",
            "openai_assistant_id": "",
            "openai_assistant_created_on": "2024-10-18T00:00:00.0000000\u002B00:00",
            "conversations": {}
        }
    },
    "properties": null,
    "created_on": "2024-10-18T00:00:00.0000000\u002B00:00",
    "updated_on": "2024-10-18T00:00:00.0000000\u002B00:00",
    "created_by": "example@solliance.net",
    "updated_by": "example@solliance.net",
    "deleted": false,
    "expiration_date": null,
}
```

*File user context*
```json
{
    "name": "example_solliance_net-file-00000000-0000-0000-0000-000000000000",
    "object_id": "/instances/00000000-0000-0000-0000-000000000000/providers/FoundationaLLM.AzureOpenAI/fileUserContexts/example_solliance_net-file-00000000-0000-0000-0000-000000000000",
    "display_name": null,
    "description": null,
    "cost_center": null,
    "user_principal_name": "example@solliance.net",
    "agent_files": {
        "/instances/00000000-0000-0000-0000-000000000000/providers/FoundationaLLM.Agent/agents/GPT-4o": {
            "endpoint": "https://example.openai.azure.com/",
            "files": {}
        }
    },
    "assistant_user_context_name": "example_solliance_net-assistant-00000000-0000-0000-0000-000000000000",
    "created_on": "2024-10-18T00:00:00.0000000+00:00",
    "updated_on": "2024-10-18T00:00:00.0000000+00:00",
    "created_by": "example@solliance.net",
    "updated_by": "example@solliance.net",
    "deleted": false,
    "expiration_date": null
}
```

Refer to the dedicated upgrade tool for instructions on how to perform this update.

## Starting with 0.8.3

### Resource provider changes

If a user/group is not assigned to the instance-level Contributor role, then they will not be able to create new Conversations or upload Attachments. To adjust their permissions, the following changes are required:

**FoundationaLLM.Conversation**

In addition to assigning users/groups to the `policy-assignments/<instance_id>-policy.json` file within the `FoundationaLLM.Authorization` resource provider to assign them to the Conversation policy, we must now add them to the new **Conversation contributor role** (`role_definition_id`: `d0d21b90-5317-499a-9208-3a6cb71b84f9`) within the `role-assignments/<instance_id>-role.json` file within the `FoundationaLLM.Authorization` resource provider if the user/group is not assigned to the Contributor role on the FoundationaLLM instance (`role_definition_id`: `a9f0020f-6e3a-49bf-8d1d-35fd53058edf`). Here is an example entry:

```json
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "a40b15f1-75ce-4a40-a857-1093ac9adf4d",
    "object_id": "/instances/0a1840df-71b6-496d-905a-145d93d827f3/providers/FoundationaLLM.Authorization/roleAssignments/a40b15f1-75ce-4a40-a857-1093ac9adf4d",
    "display_name": null,
    "description": "Conversation contributor role for FLLM Users",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/d0d21b90-5317-499a-9208-3a6cb71b84f9",
    "principal_id": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
    "principal_type": "Group",
    "scope": "/instances/0a1840df-71b6-496d-905a-145d93d827f3",
    "properties": null,
    "created_on": "0001-01-01T00:00:00+00:00",
    "updated_on": "0001-01-01T00:00:00+00:00",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
}
```

**FoundationaLLM.Attachment**

In addition to assigning users/groups to the `policy-assignments/<instance_id>-policy.json` file within the `FoundationaLLM.Authorization` resource provider to assign them to the Attachment policy, we must now add them to the new **Attachment contributor role** (`role_definition_id`: `8e77fb6a-7a78-43e1-b628-d9e2285fe25a`) within the `role-assignments/<instance_id>-role.json` file within the `FoundationaLLM.Authorization` resource provider if the user/group is not assigned to the Contributor role on the FoundationaLLM instance (`role_definition_id`: `a9f0020f-6e3a-49bf-8d1d-35fd53058edf`). Here is an example entry:

```json
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "891ca947-e648-46cf-a12a-774b52ded886",
    "object_id": "/instances/0a1840df-71b6-496d-905a-145d93d827f3/providers/FoundationaLLM.Authorization/roleAssignments/891ca947-e648-46cf-a12a-774b52ded886",
    "display_name": null,
    "description": "Attachment contributor role for FLLM Users",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/8e77fb6a-7a78-43e1-b628-d9e2285fe25a",
    "principal_id": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
    "principal_type": "Group",
    "scope": "/instances/0a1840df-71b6-496d-905a-145d93d827f3",
    "properties": null,
    "created_on": "0001-01-01T00:00:00+00:00",
    "updated_on": "0001-01-01T00:00:00+00:00",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
}
```

## Starting with 0.8.2

### Configuration changes

The following settings are required:

Name | Default value
--- | ---
`FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AllowedUploadFileExtensions` | `c, cpp, cs, css, csv, doc, docx, gif, html, java, jpeg, jpg, js, json, md, pdf, php, png, pptx, py, rb, sh, tar, tex, ts, txt, xlsx, xml, zip`
`FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AzureOpenAIAssistantsFileSearchFileExtensions` | `c, cpp, cs, css, doc, docx, html, java, js, json, md, pdf, php, pptx, py, rb, sh, tex, ts, txt`
`FoundationaLLM:APIEndpoints:CoreAPI:Configuration:MaxUploadsPerMessage` |	`{ "value": 10, "value_exceptions": [] }`
`FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CompletionResponsePollingIntervalSeconds` | `{ "value": 5, "value_exceptions": [] }`
`FoundationaLLM:APIEndpoints:GatewayAPI:Configuration:AzureOpenAIAssistantsMaxVectorizationTimeSeconds` | `120`

>[!NOTE]
> Here is an example of an override for the `MaxUploadsPerMessage` setting:
> ```json
>{
>   "value": 10,
>   "value_exceptions": [
>       {
>           "user_principal_name": "ciprian@solliance.net",
>           "value": 5,
>           "enabled": true
>       }
>   ]
>}
> ```

>[!NOTE]
> Here is an example of an override for the `CompletionResponsePollingIntervalSeconds` setting:
> ```json
>{
>   "value": 5,
>   "value_exceptions": [
>       {
>           "user_principal_name": "ciprian@solliance.net",
>           "value": 3,
>           "enabled": true
>       }
>   ]
>}
>

The following settings are optional (they should not be set by default):

Name | Default value
--- | ---
`FoundationaLLM:Instance:IdentitySubstitutionSecurityPrincipalId` | <security_principal_id>
`FoundationaLLM:Instance:IdentitySubstitutionUserPrincipalNamePattern` | `^fllm_load_test_user_\d{5}_\d{3}@solliance\.net$`

>[!NOTE]
> The `FoundationaLLM:Instance:IdentitySubstitutionSecurityPrincipalId` and `FoundationaLLM:Instance:IdentitySubstitutionUserPrincipalNamePattern` settings are used for load testing purposes only. If set, their values must be replaced with the appropriate values for the specific Entra ID tenant.

### Resource provider changes

The following resource provider files must be renamed (if they already exist):

Location | Old name | New name
--- | --- | ---
`resource-provider/FoundationaLLM.Agent` | `_agent-references.json` | `_resource-references.json`
`resource-provider/FoundationaLLM.AIModel` | `_ai-model-references.json` | `_resource-references.json`
`resource-provider/FoundationaLLM.Configuration` | `_api-endpoint-references.json` | `_resource-references.json`
`resource-provider/FoundationaLLM.DataSource` | `_data-source-references.json` | `_resource-references.json`
`resource-provider/FoundationaLLM.Prompt` | `_prompt-references.json` | `_resource-references.json`

>[!NOTE]
> Within each of the renamed files, the `<entity>References` property must be renamed to `ResourceReferences`.

**FoundationaLLM.Agent**

A new property can be added to agent definitions:

```json
"tools": {
    "dalle-image-generation": {
        "name": "dalle-image-generation",
        "description": "Generates an image based on a prompt.",
        "ai_model_object_ids": {
            "main_model": "/instances/73fad442-f614-4510-811f-414cb3a3d34b/providers/FoundationaLLM.AIModel/aiModels/DALLE3"
        }
    }
}
```

**FoundationaLLM.Authorization**

A new storage container named `policy-assignments` is required. The `FoundationaLLM.Authorization` resource provider will use this container to store policy assignments.

Within the container, the `<instance_id>-policy.json` must be deployed with the default policy assignments. The template for the default policy assignments is available in `Common/Constants/Data/DefaultPolicyAssignments.json`.

**FoundationaLLM.Conversation**

When upgrading an existing FoundationaLLM instance, the items in the `Sessions` collection in Cosmos DB must be updated according to the following rules:

```
if Object is of type Session of KioskSession:
    If the property DisplayName exists and is set to a non-empty string:
        Don't touch the item
    else:
        Set DisplayName to the value of Name
        Set Name to the value of SessionId
else:
    No action needed
```

Refer to the dedicated upgrade tool for instruction on how to perform this update.

**FoundationaLLM.Configuration**

The OneDrive (Work or School) integration requires the following API Endpoint Configuration entry in the storage account:

`FoundationaLLM.Configuration/OneDriveFileStoreConnector.json`
```json
{
    "type": "api-endpoint",
    "name": "OneDriveFileStoreConnector",
    "object_id": "/instances/{{instance_id}}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/OneDriveFileStoreConnector",
    "display_name": null,
    "description": null,
    "cost_center": null,
    "category": "FileStoreConnector",
    "subcategory": "OneDriveWorkSchool",
    "authentication_type": "AzureIdentity",
    "authentication_parameters": {
      "scope": "Files.Read.All"
    },
    "url": "{{onedrive_base_url}}",
    "status_url": "",
    "url_exceptions": [],
    "timeout_seconds": 2400,
    "retry_strategy_name": "ExponentialBackoff",
    "created_on": "0001-01-01T00:00:00+00:00",
    "updated_on": "0001-01-01T00:00:00+00:00",
    "created_by": null,
    "updated_by": "SYSTEM",
    "deleted": false
  }
```

Update `FoundationaLLM.Configuration/_resource-references_.json` with the reference to the file above.
```json
{
	"Name": "OneDriveFileStoreConnector",
	"Filename": "/FoundationaLLM.Configuration/OneDriveFileStoreConnector.json",
	"Type": "api-endpoint",
	"Deleted": false
}
```

**FoundationaLLM.Attachment**

The Attachment resource provider now saves the attachment references to Cosmos DB, instead of Data Lake storage.
A new Cosmos DB container must be created, named `Attachments`, with the following partition key: `/upn`.

The following MSIs require a Cosmos DB role assigned:
1. Gateway API
2. Orchestration API
3. Management API

### Long-Running Operations

The context for a long-running operation is now stored in Cosmos DB.
A new Cosmos DB container must be created, named `Operations`, with a partition key `/id`.


## Starting with 0.8.0

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
10. Vectorization Embedding Profile introduces a required key in the `Settings` property named `model_name`. Every embedding request now flows through the Gateway API.
11. Vectorization Indexing Profile introduces a required key `api_endpoint_configuration_object_id` in the `Settings` property.
12. Retirement of `SemanticKernel` embedding type. All embedding requests now flow through the Gateway API.

Gatekeeper API changes:
1. All Gatekeeper API endpoints have been moved to the `/instances/{instanceId}` path. For example, the `/status` endpoint is now `/instances/{instanceId}/status`.
2. The `/orchestration/*` endpoints have been moved to `/instances/{instanceId}/completions/*`.

Orchestration API changes:
1. All Gatekeeper API endpoints have been moved to the `/instances/{instanceId}` path. For example, the `/status` endpoint is now `/instances/{instanceId}/status`.
2. The `/orchestration/*` endpoints have been moved to `/instances/{instanceId}/completions/*`.
=======
### New APIs

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

### Changes in app registration names

API Name | Entra ID app registration name | Application ID URI | Scope name
--- | --- | --- | ---
Core API | `FoundationaLLM-Core-API` | `api://FoundationaLLM-Core` | `Data.Read`
Management API | `FoundationaLLM-Management-API` | `api://FoundationaLLM-Management` | `Data.Manage`
Authorization API | `FoundationaLLM-Authorization-API` | `api://FoundationaLLM-Authorization` | `Authorization.Manage`
User Portal | `FoundationaLLM-Core-Portal` | `api://FoundationaLLM-Core-Portal` | N/A
Management Portal | `FoundationaLLM-Management-Portal` | `api://FoundationaLLM-Management-Portal` | N/A

### Changes in app configuration settings

The `FoundationaLLM:APIs` and `FoundationaLLM:ExternalAPIs` configuration namespaces have been replaced with the `FoundationaLLM:APIEndpoints` configuration namespace.

> [!IMPORTANT]
> All existing API registrations need to be updated to reflect these changes. The only two settings that will exist under `FoundationaLLM:APIEndpoints` are `APIKey` (for those API enpoints which use API key authentication) and `AppInsightsConnectionString`, all the other settings are now part of the `APIEndpoint` artifact managed by the `FoundationaLLM.Configuration` resource provider.
> This is an example for `CoreAPI`:
> - `FoundationaLLM:APIEndpoints:CoreAPI:APIKey`
> - `FoundationaLLM:APIEndpoints:CoreAPI:AppInsightsConnectionString`

The `FoundationaLLM:AzureAIStudio` configuration namespace expects an `APIEndpointConfigurationName` property instead of `BaseUrl`.

A new configuration setting named `FoundationaLLM:Instance:SecurityGroupRetrievalStrategy` with a value of `IdentityManagementService` must exist in the app configuration. It will be added by default in new deployments.

Two new configuration settings required by the new `FoundationaLLM.AzureOpenAI` resource provider:
- `FoundationaLLM:ResourceProviders:AzureOpenAI:Storage:AuthenticationType`
- `FoundationaLLM:ResourceProviders:AzureOpenAI:Storage:AccountName`

## Pre-0.8.0

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



