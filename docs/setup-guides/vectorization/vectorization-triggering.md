# Triggering vectorization

Vectorization pipelines are started when the Vectorization API receives a vectorization request. The following types of triggers are supported:

- None (no triggering of vectorization pipelines).
- Manual (vectorization pipelines are triggered manually by calling the Vectorization API). The typical use cases for on-demand vectorization (either synchronous or asynchronous) are testing, manual vectorization (or re-vectorization), and application integration (where another platform component triggers vectorization).
- Content-based (vectorization pipelines are triggered automatically when either new content is added to a content source or existing content is updated).
- Schedule-based (vectorization pipelines are triggered automatically based on a schedule).

> [!NOTE]
> Content-based and schedule-based triggering are currently in pre-release and are not yet available in public releases of FLLM.

## Vectorization requests

The typical structure of a vectorization request is the following

```json
{
    "content_identifier": {
        "content_source_profile_name": "<name>",
        "multipart_id": [
            "xyz.blob.core.windows.net",
            "vectorization-input",
            "The-fabulous-life-of-Jack-the-Cat.pdf"
        ],
        "canonical_id": "friends/stories/The-fabulous-life-of-Jack-the-Cat"
    },
    "processing_type": "Asynchronous",
    "steps": [
        {
            "id": "extract",
            "parameters": {}
        },
        {
            "id": "partition",
            "parameters": {
                "text_partition_profile_name": "DefaultTokenTextPartition"
            }
        },
        {
            "id": "embed",
            "parameters": {
                "text_embedding_profile_name": "AzureOpenAI_Embedding"
            }
        },
        {
            "id": "index",
            "parameters": {
                "indexing_profile_name": "AzureAISearch_Default_001"
            }
        }
    ]
}
```

The following table describes the properties of a vectorization request.

| Property | Description |
| --- | --- |
| `content_identifier` | The content identifier of the content to be vectorized. |
| `content_identifier.content_source_profile_name` | The name of the content source profile to be used for loading the content. |
| `content_identifier.multipart_id` | The multipart ID of the content to be vectorized. The multipart ID is a list of strings that uniquely identifies the content. The multipart ID is specific to the content source profile. |
| `content_identifier.canonical_id` | The canonical ID of the content to be vectorized. The canonical ID is a string that uniquely identifies the content in a logical namespace. The caller is responsible for the generation of this identifier. The identifier should have a path form (using the `/` separator). The last part of the path should always be equal to the file name (without its extension). |
| `processing_type` | The type of processing to be performed. The following values are supported: `Synchronous` and `Asynchronous`. See [Vectorization concepts](./vectorization-concepts.md) for more details. |
| `steps` | The vectorization steps to be executed. Most vectorization requests will contain the full set of standard steps: `extract`, `partition`, `embed`, and `index`. Each step (except for the `extract` one) will contain one parameter specifying the name of the associated vectorization profile name. |

The meaning of the multipart strings depends on the specific type of the content source in the content source profile.

The following table describes the meaning of the multipart strings for the `AzureDataLake` content source.

| Position | Description |
| --- | --- |
| 1 | The URL of the Azure Data Lake storage account. When providing this value, you can use the [known neutral URLs](#known-neutral-urls) naming conventions. |
| 2 | The name of the container. |
| 3 | The path of the file relative to the container. |

The following table describes the meaning of the multipart strings for the `SharePointOnline` content source.

| Position | Description |
| --- | --- |
| 1 | The URL of the SharePoint Online tenant. When providing this value, you can use the [known neutral URLs](#known-neutral-urls) naming conventions. |
| 2 | The path of the site/subsite relative to the tenant URL. |
| 3 | The folder path, starting with the document library name. |
| 4 | The name of the file. |

The following table describes the meaning of the multipart strings for the `AzureSQLDatabase` content source.

| Position | Description |
| --- | --- |
| 1 | The name of the database schema. |
| 2 | The name of the table. |
| 3 | The name of the column that stores file content. |
| 4 | The name of the column that stores file identifiers (file names). |
| 5 | The file identifier (file name). |


### Known neutral URLs

Depending on the specific configuration of various layers of security, vectorization request might end up being filtered out by infrastructure components like firewalls or proxies. To avoid this, the Vectorization API supports the use of known neutral URLs. 

Known neutral URLs are URLs that have a neutral form that is not subject to filtering. The platform currently supports two conventions for specifing known neutral URLs:

- Simple: `xyz.blob.core.windows.net` - this will be translated into `https://xyz.blob.core.windows.net` by the platform.
- Complex: `FLLM:xyz#blob#core#windows#net` - this will be translated into `https://xyz.blob.core.windows.net` by the platform.

Depending on the specific configuration of the infrastructure, one of the two conventions might be more suitable than the other.

> [!NOTE]
> To avoid bypassing security measures, the use of known neutral URLs is currently restricted to the following domains: `onelake.dfs.fabric.microsoft.com`, `blob.core.windows.net`, `dfs.core.windows.net`, and `sharepoint.com`.

When parsing multipart components that are subject to known neutral URL naming conventions, the platform will apply the following logic:

1. If the component starts with `https://` or `http://` (case-insensitive), the platform will not apply any transformation since the explicit intent is to use a fully qualified URL.
2. If the component starts with `FLLM:`, the platform will replace the `FLLM:` prefix with `https://` and replace all `#` characters with `.`. Then, it will check if the tail of the resulting URL is in the list of allowed domains. If it is, the platform will use the resulting URL. If it is not, the platform will use the original form of the component.
3. At this point, the platform will assume that the component is a simple known neutral URL and will prepend `https://` to it. Then, if the tail of the resulting URL is in the list of allowed domains, the platform will use the resulting URL. If it is not, the platform will use the original form of the component.

### Submitting vectorization requests

This section describes how to submit vectorization requests using the Vectorization API.
`{{baseUrl}}` is the base URL of the Vectorization API.

```
POST {{baseUrl}}/vectorizationrequest
Content-Type: application/json
X-API-KEY: <vectorization_api_key>

BODY
<vectorization_request>
```

where <vectorization_api_key> is the API key of the Vectorization API and `<vectorization_request>` is the vectorization request to be submitted.

Upon completion, the API will return a response with the following structure:

```json
{
    "object_id":"/instances/1e22cd2a-7b81-4160-b79f-f6443e3a6ac2/providers/FoundationaLLM.Vectorization/vectorizationrequests/6041849a-d4d8-428d-97ff-c6a3443ecdae",
    "is_success":true,
    "error_message":null
}
```
