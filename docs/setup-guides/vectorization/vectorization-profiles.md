# Managing vectorization profiles

The FoundationaLLM (FLLM) vectorization pipelines required the following types of profiles:

- [Content source profiles](#content-source-profiles)
- [Text partitioning profiles](#text-partitioning-profiles)
- [Text embedding profiles](#text-embedding-profiles)
- [Indexing profiles](#indexing-profiles)

## Content source profiles

The structure of a content source profile is the following:

```json
{
    "Name": "<name>",
    "Type": "<type>",
    "ObjectId": "/instances/<instance_id>/providers/FoundationaLLM.Vectorization/contentsourceprofiles/<name>",
    "Settings": {<profile_settings>},
    "ConfigurationReferences": {<profile_configuration_references>}
}
```
where:

- `<name>` is the name of the content source profile.
- `<type>` is the type of the content source profile. The supported types are `AzureDataLake`, `SharePointOnline`, and `AzureSQLDatabase`.
- `<profile_settings>` is a JSON object containing the profile settings.
- `<profile_configuration_references>` is a JSON object containing the profile configuration references.

The reminder of this section describes the configuration parameters for each of the supported content source types.

### `AzureDataLake`

```json
"Settings": {},
"ConfigurationReferences": {
    "AuthenticationType": "FoundationaLLM:Vectorization:ContentSources:<content_source_name>:AuthenticationType",
    "ConnectionString": "FoundationaLLM:Vectorization:ContentSources:<content_source_name>:ConnectionString"
}
```

The configuration parameters for `AzureDataLake` are the following:

| Parameter | Description |
| --- | --- |
| `ConfigurationReferences.AuthenticationType` | The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`. |
| `ConfigurationReferences.ConnectionString` | The connection string to the Azure Storage account used for the the Azure Data Lake vectorization content source. |

### `SharePointOnline`

```json
"Settings": {},
"ConfigurationReferences": {
    "CertificateName": "FoundationaLLM:Vectorization:ContentSources:<content_source_name>:CertificateName",
    "ClientId": "FoundationaLLM:Vectorization:ContentSources:<content_source_name>:ClientId",
    "KeyVaultURL": "FoundationaLLM:Vectorization:ContentSources:<content_source_name>:KeyVaultURL",
    "TenantId": "FoundationaLLM:Vectorization:ContentSources:<content_source_name>:TenantId"
}
```

The configuration parameters for `SharePointOnline` are the following:

| Parameter | Description |
| --- | --- |
| `ConfigurationReferences.CertificateName` | The name of the X.509 Certificate. The certificate must be valid and be uploaded into an Azure Key Vault certificate store. |
| `ConfigurationReferences.KeyVaultURL` | The URL of the KeyVault where the X.509 Certificate is stored. |
| `ConfigurationReferences.ClientId` | The Application (client) Id of the Microsoft Entra ID App Registration. See [Entra ID app registration for SharePoint Online content source](#entra-id-app-registration-for-sharepoint-online-content-source). |
| `ConfigurationReferences.TenantId` | The unique identifier of the SharePoint Online tenant. |

### `AzureSQLDatabase`

```json
"Settings": {},
"ConfigurationReferences": {
    "ConnectionString": "FoundationaLLM:Vectorization:ContentSources:<content_source_name>:ConnectionString"
}
```

The configuration parameters for `AzureSQLDatabase` are the following:

| Parameter | Description |
| --- | --- |
| `ConfigurationReferences.ConnectionString` | The connection string to the Azure SQL database used for the Azure SQL Database vectorization content source. |

### Managing content source profiles

This section describes how to manage content source profiles using the Management API.
`{{baseUrl}}` is the base URL of the Management API. `{{instanceId}}` is the unique identifier of the FLLM instance.

**Retrieve**

```
HTTP GET {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/contentsourceprofiles
```

**Create or update**

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/contentsourceprofiles/<name>
Content-Type: application/json

BODY
<content source profile>
```
where `<content source profile>` is a JSON object with the structure described above.

**Delete**

```
HTTP DELETE {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/contentsourceprofiles/<name>
```

## Text partitioning profiles

The structure of a text partitioning profile is the following:

```json
{
    "Name": "<name>",
    "TextSplitter": "<text_splitter>",
    "ObjectId": "/instances/<instance_id>/providers/FoundationaLLM.Vectorization/textpartitioningprofiles/<name>",
    "Settings": {<profile_settings>},
    "ConfigurationReferences": {<profile_configuration_references>}
}
```
where:

- `<name>` is the name of the text partitioning profile.
- `<text_splitter>` is the type of the text splitter. The supported types are `TextTokenSplitter`.
- `<profile_settings>` is a JSON object containing the profile settings.
- `<profile_configuration_references>` is a JSON object containing the profile configuration references.

The reminder of this section describes the configuration parameters for each of the supported text splitters.

### `TextTokenSplitter`

```json
"Settings" : {
    "Tokenizer": "MicrosoftBPETokenizer",
    "TokenizerEncoder": "cl100k_base",
    "ChunkSizeTokens": "2000",
    "OverlapSizeTokens": "200"
},
"ConfigurationReferences": {}
```

The configuration parameters for `TokenTextSplitter` are the following:

| Parameter | Description |
| --- | --- |
| `Tokenizer` | The tokenizer used to split the text into tokens. Currently, the only supported tokenizer is `MicrosoftBPETokenizer`. Under the hood, it uses the .NET equivalent of OpenAI's [tiktoken](https://github.com/openai/tiktoken). |
| `TokenizerEncoder` | The encoder used by the tokenizer. Currently, the only supported encoder is `cl100k_base`. This encoder is the one currently used by Azure OpenAI (and OpenAI) in `gpt-3.5-turbo` and `gpt-4`. |
| `ChunkSizeTokens` | The maximum number of tokens in each text chunk. |
| `OverlapSizeTokens` | The maximum number of tokens that overlap between two consecutive chunks. |

### Managing text partitioning profiles

This section describes how to manage text partitioning profiles using the Management API.
`{{baseUrl}}` is the base URL of the Management API. `{{instanceId}}` is the unique identifier of the FLLM instance.

**Retrieve**

```
HTTP GET {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/textpartitioningprofiles
```

**Create or update**

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/textpartitioningprofiles/<name>
Content-Type: application/json

BODY
<text partitioning profile>
```

where `<text partitioning profile>` is a JSON object with the structure described above.

**Delete**

```
HTTP DELETE {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/textpartitioningprofiles/<name>
```

## Text embedding profiles

The structure of a text embedding profile is the following:

```json
{
    "Name": "<name>",
    "TextEmbedding": "<text_embedder>",
    "ObjectId": "/instances/<instance_id>/providers/FoundationaLLM.Vectorization/textembeddingprofiles/<name>",
    "Settings": {<profile_settings>},
    "ConfigurationReferences": {<profile_configuration_references>}
}
```
where:

- `<name>` is the name of the content source profile.
- `<text_embedding>` is the type of the text embedder. The supported types are `SemanticKernelTextEmbedding`.
- `<profile_settings>` is a JSON object containing the profile settings.
- `<profile_configuration_references>` is a JSON object containing the profile configuration references.

The reminder of this section describes the configuration parameters for each of the supported text embedders.

### `SemanticKernelTextEmbedding`

```json
"Settings": {},
"ConfigurationReferences": {
    "APIKey": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIKey",
    "AuthenticationType": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:AuthenticationType",
    "DeploymentName": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:DeploymentName",
    "Endpoint": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:Endpoint"
}
```

The configuration parameters for `SemanticKernelTextEmbedding` are the following:

| Parameter | Description |
| --- | --- |
| `ConfigurationReferences.APIKey` | The API key used to connect to the Azure OpenAI service. By default, this maps to the Azure OpenAI service deployed by FLLM.|
| `ConfigurationReferences.AuthenticationType` | The authentication type used to connect to the Azure OpenAI service. Can be one of `AzureIdentity` or `APIKey`. By default, it is set to `APIKey`.|
| `ConfigurationReferences.DeploymentName` | The name of the Azure OpenAI model deployment. The default value is `embeddings`.|
| `ConfigurationReferences.Endpoint` | The endpoint of the Azure OpenAI service. By default, this maps to the Azure OpenAI service deployed by FLLM. |

### Managing text embedding profiles

This section describes how to manage text embedding profiles using the Management API.
`{{baseUrl}}` is the base URL of the Management API. `{{instanceId}}` is the unique identifier of the FLLM instance.

**Retrieve**

```
HTTP GET {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/textembeddingprofiles
```

**Create or update**

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/textembeddingprofiles/<name>
Content-Type: application/json

BODY
<text embedding profile>
```

where `<text embedding profile>` is a JSON object with the structure described above.

**Delete**

```
HTTP DELETE {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/textembeddingprofiles/<name>
```

## Indexing profiles

The structure of an indexing profile is the following:

```json
{
    "Name": "<name>",
    "Indexer": "<indexer>",
    "ObjectId": "/instances/<instance_id>/providers/FoundationaLLM.Vectorization/indexingprofiles/<name>",
    "Settings": {<profile_settings>},
    "ConfigurationReferences": {<profile_configuration_references>}
}
```
where:

- `<name>` is the name of the content source profile.
- `<indexer>` is the type of the indexer. The supported types are `AzureAISearchIndexer`.
- `<profile_settings>` is a JSON object containing the profile settings.
- `<profile_configuration_references>` is a JSON object containing the profile configuration references.

The reminder of this section describes the configuration parameters for each of the supported indexers.

### `AzureAISearchIndexer`

```json
"Settings": {
    "IndexName": "fllm-default-001"
},
"ConfigurationReferences": {
    "APIKey": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey",
    "AuthenticationType": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType",
    "Endpoint": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint"
}
```

The configuration parameters for `AzureAISearchIndexer` are the following:

| Parameter | Description |
| --- | --- |
| `Settings.IndexName` | The name of the Azure AI Search index. If the index does not exist, it will be created. |
| `ConfigurationReferences.APIKey` | The API key used to connect to the Azure AI Search service. By default, this maps to the Azure AI Search service deployed by FLLM.|
| `ConfigurationReference.AuthenticationType` | The authentication type used to connect to the Azure AI Search service. Can be one of `AzureIdentity` or `APIKey`. By default, it is set to `APIKey`.|
| `ConfigurationReferences.Endpoint` | The endpoint of the Azure AI Search service. By default, this maps to the Azure AI Search service deployed by FLLM. |

### Managing indexing profiles

This section describes how to manage indexing profiles using the Management API.
`{{baseUrl}}` is the base URL of the Management API. `{{instanceId}}` is the unique identifier of the FLLM instance.

**Retrieve**

```
HTTP GET {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/indexingprofiles
```

**Create or update**

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/indexingprofiles/<name>
Content-Type: application/json

BODY
<indexing profile>
```

where `<indexing profile>` is a JSON object with the structure described above.

**Delete**

```
HTTP DELETE {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/indexingprofiles/<name>
```

## Additional configuration steps

### Entra ID app registration for SharePoint Online content source

Apps typically access SharePoint Online through certificates: Anyone having the certificate and its private key can use the app with the permissions granted to it.

1. Create a new **App registration** in your **Microsoft Entra ID** tenant. Next, provide a **Name** for your application and click on **Register** at the bottom of the blade.

2. Navigate to the **API Permissions** blade and click on **Add a permission** button Here you choose the permissions that you will grant to this application. Select **SharePoint** from the **Microsoft APIs** tab, then select **Application permissions** as the type of permissions required, choose the desired permissions (i.e. **Sites.Read.All**) and click on **Add permissions**. Here are the required scopes:

    - `Group.ReadWrite.All`
    - `User.ReadWrite.All`
    - `Sites.Read.All` OR `Sites.Selected`
      - `Sites.Read.All` will allow the application to read documents and list items in all site collections.
      - `Sites.Selected` will allow the application to access only a subset of site collections. The specific site collections and the permissions granted will be configured separately, in SharePoint Online.

3. The application permission requires admin consent in a tenant before it can be used. In order to do this, click on **API permissions** in the left menu again. At the bottom you will see a section **Grant consent**. Click on the **Grant admin consent for {{organization}}** button and confirm the action by clicking on the **Yes** button that appears at the top.

4. To invoke SharePoint Online with an app-only access token, you have to create and configure a **self-signed X.509 certificate**, which will be used to authenticate your application against Microsoft Entra ID. You can find additional details on how to do this in [this document](https://learn.microsoft.com/en-us/sharepoint/dev/solution-guidance/security-apponly-azuread#setting-up-an-azure-ad-app-for-app-only-access).

5. Next step is to register the certificate you created to this application. Click on **Certificates & secrets** blade. Next, click on the **Upload certificate** button, select the .CER file you generated earlier and click on **Add** to upload it. 

    To confirm that the certificate was successfully registered, click on **Manifest** blade and search for the `keyCredentials` property, which contains your certificate details. It should look like this:
    ```json
    "keyCredentials": [
        {
            "customKeyIdentifier": "<$base64CertHash>",
            "endDate": "yyyy-MM-ddThh:mm:ssZ",
            "keyId": "<$guid>",
            "startDate": "yyyy-MM-ddThh:mm:ssZ",
            "type": "AsymmetricX509Cert",
            "usage": "Verify",
            "value": "<$base64Cert>",
            "displayName": "CN=<$name of your cert>"
        }
    ]
    ```

6. Upload and store the certificate in the **KeyVault** where the FoundationaLLM Vectorization API has permissions to read **Secrets**. You will need the **Certificate Name** for the App Configuration settings listed in the table above.

    > **NOTE**
    >
    > Can I use other means besides certificates for realizing app-only access for my Azure AD app?
    >
    > **NO**, all other options are blocked by SharePoint Online and will result in an `Access Denied` message.

7. Navigate to the deployment storage account and edit the `resource-provider/FoundationaLLM.Vectorization/vectorization-content-source-profiles.json` file. Append the following snippet to the `ContentSourceProfiles` array.

    ```json
    {
        "Name": "[CONTENT SOURCE NAME]",
        "Type": "SharePointOnline"
    }
    ```