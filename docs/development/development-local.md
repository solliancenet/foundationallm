# Configure local development environment

## Requirements

## Chat

#### Make sure the contents of the `appsettings.json` file has this structure and similar values:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.SemanticKernel": "Error"
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.SemanticKernel": "Error"
      }
    }
  },
  "AllowedHosts": "*",
  "FoundationaLLM": {
    "ChatManager": {
      "APIUrl": "https://localhost:63279",
      "APIRoutePrefix": ""
    },
    "Configuration": {
      "KeyVaultUri": ""
    },
    "Entra": {
      "Instance": "https://login.microsoftonline.com/",
      "TenantId": "",
      "ClientId": "",
      "CallbackPath": "/signin-oidc",
      "ClientSecretKeyName": "",
      "Scopes": ""
    }
  }
}
```

#### Create the `appsettings.Development.json` file or update it with the following content and replace all `<...>` placeholders with the values from your deployment:

```json
{
  "FoundationaLLM": {
    "Configuration": {
      "KeyVaultUri": "https://<...>.vault.azure.net/"
    },
    "Entra": {
      "TenantId": "<...>",
      "ClientId": "<...>",
      "ClientSecretKeyName": "<...>",
      "Scopes": "<...>"
    }
  }
}
```

## Core API

#### Make sure the contents of the `appsettings.json` file has this structure and similar values:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.SemanticKernel": "Error"
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.SemanticKernel": "Error"
      }
    }
  },
  "AllowedHosts": "*",
  "FoundationaLLM": {
    "CosmosDB": {
      "Containers": "completions, customer, product",
      "MonitoredContainers": "customer, product",
      "Database": "database",
      "ChangeFeedLeaseContainer": "leases"
    },
    "GatekeeperAPI": {
      "APIUrl": ""
    },
    "Configuration": {
      "KeyVaultUri": ""
    },
    "Entra": {
      "Instance": "https://login.microsoftonline.com/",
      "TenantId": "",
      "ClientId": "",
      "CallbackPath": "/signin-oidc",
      "ClientSecretKeyName": "",
      "Scopes": ""
    }
  }
}
```

#### Create the `appsettings.Development.json` file or update it with the following content and replace all `<...>` placeholders with the values from your deployment:

```json
{
  "FoundationaLLM": {
    "CosmosDB": {
      "Endpoint": "https://<...>-cosmos-nosql.documents.azure.com:443/",
      "Key": "<...>"
    },
    "GatekeeperAPI": {
      "APIUrl": "<...>"
    },
    "Configuration": {
      "KeyVaultUri": "https://<...>-keyvault.vault.azure.net/"
    },
    "Entra": {
      "TenantId": "<...>",
      "ClientId": "<...>",
      "ClientSecretKeyName": "<...>",
      "Scopes": "<...>"
    }
  }
}
```

## Gatekeeper API

#### Make sure the contents of the `appsettings.json` file has this structure and similar values:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.SemanticKernel": "Error"
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.SemanticKernel": "Error"
      }
    }
  },
  "AllowedHosts": "*",
  "FoundationaLLM": {
    "AgentFactoryAPI": {
      "APIUrl": ""
    },
    "GatekeeperAPI": {
      "APIKeySecretName": "foundationallm-gatekeeper-api-key"
    }
  }
}
```

#### Create the `appsettings.Development.json` file or update it with the following content and replace all `<...>` placeholders with the values from your deployment:

```json
{
  "FoundationaLLM": {
    "AgentFactoryAPI": {
      "APIUrl": "<...>"
    }
  }
}
```

## Agent Factory API

#### Make sure the contents of the `appsettings.json` file has this structure and similar values:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.SemanticKernel": "Error"
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.SemanticKernel": "Error"
      }
    }
  },
  "AllowedHosts": "*",
  "FoundationaLLM": {
    "Chat": {
      "DefaultOrchestrationService": "SemanticKernel"
    },
    "LangChainOrchestration": {
      "APIUrl": ""
    },
    "SemanticKernelOrchestration": {
      "APIUrl": ""
    },
    "AgentFactoryAPI": {
      "APIKeySecretName": "foundationallm-agentfactory-api-key"
    }
  }
}
```

#### Create the `appsettings.Development.json` file or update it with the following content and replace all `<...>` placeholders with the values from your deployment:

```json
{
  "FoundationaLLM": {
    "LangChainOrchestration": {
      "APIUrl": "<...>"
    },
    "SemanticKernelOrchestration": {
      "APIUrl": "<...>"
    }
  }
}
```

## PythonSDK

### Environment Variables

| Name | Value | Description |
| ---- | ----- | ----------- |
| foundationallm-keyvault-name | REDACTED | Key Vault name |
| foundationallm-configuration-allow-environment-variables | True | Allow environment variables |
| foundationallm-agent-metadata-storage-container | agent | Agent metadata storage container name |
| foundationallm-datasource-metadata-storage-container | data-sources | Datasource metadata storage container name |
| foundationallm-prompt-metadata-storage-container | system-prompt | Prompt metadata storage container name |
| foundationallm-azure-openai-api-url | REDACTED | Azure Open AI instance URL |
| foundationallm-azure-openai-api-completions-deployment | completions | Azure Open AI completions deployment name |
| foundationallm-azure-openai-api-completions-model-version | 0301 | Azure Open AI completions model version |
| foundationallm-azure-openai-api-version | 2023-07-01-preview | Azure Open AI API version |
| foundationallm-langchain-summary-model-name | gpt-35-turbo | LangChain summary model name |
| foundationallm-langchain-summary-model-max-tokens | 4097 | LangChain summary model max tokens |
| foundationallm-langchain-sqldb-testdb-server-name | REDACTED | Temporary SQL Server name for testing |
| foundationallm-langchain-sqldb-testdb-database-name | REDACTED | Temporary SQL Database name for testing |
| foundationallm-langchain-sqldb-testdb-username | REDACTED | Temporary SQL Database user name for testing |

## Agent Hub API

### Environment Variables

| Name | Value | Description |
| ---- | ----- | ----------- |

## Data Source Hub API

### Environment Variables

| Name | Value | Description |
| ---- | ----- | ----------- |

## Prompt Hub API

### Environment Variables

| Name | Value | Description |
| ---- | ----- | ----------- |

## LangChain API

### Environment Variables

| Name | Value | Description |
| ---- | ----- | ----------- |

## Semantic Kernel API

#### Make sure the contents of the `appsettings.json` file has this structure and similar values:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.SemanticKernel": "Error"
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.SemanticKernel": "Error"
      }
    }
  },
  "AllowedHosts": "*",
  "FoundationaLLM": {
    "CognitiveSearch": {
      "IndexName": "vector-index",
      "MaxVectorSearchResults": 10
    },
    "OpenAI": {
      "CompletionsDeployment": "completions",
      "CompletionsDeploymentMaxTokens": 8096,
      "EmbeddingsDeployment": "embeddings",
      "EmbeddingsDeploymentMaxTokens": 8191,
      "ChatCompletionPromptName": "RetailAssistant.Default",
      "ShortSummaryPromptName": "Summarizer.TwoWords",
      "PromptOptimization": {
        "CompletionsMinTokens": 50,
        "CompletionsMaxTokens": 300,
        "SystemMaxTokens": 1500,
        "MemoryMinTokens": 1500,
        "MemoryMaxTokens": 3000,
        "MessagesMinTokens": 100,
        "MessagesMaxTokens": 3000
      }
    },
    "DurableSystemPrompt": {
      "BlobStorageContainer": "system-prompt"
    },
    "CognitiveSearchMemorySource": {
      "IndexName": "vector-index",
      "ConfigBlobStorageContainer": "memory-source",
      "ConfigFilePath": "ACSMemorySourceConfig.json"
    },
    "BlobStorageMemorySource": {
      "ConfigBlobStorageContainer": "memory-source",
      "ConfigFilePath": "BlobMemorySourceConfig.json"
    },
    "SemanticKernelOrchestration": {
      "APIKeySecretName": "foundationallm-semantickernel-api-key"
    }
  }
}
```

#### Create the `appsettings.Development.json` file or update it with the following content and replace all `<...>` placeholders with the values from your deployment:

```json
{
  "FoundationaLLM": {
    "CognitiveSearch": {
      "Endpoint": "https://<...>-cog-search.search.windows.net",
      "Key": "<...>"
    },
    "OpenAI": {
      "Endpoint": "https://<...>-openai.openai.azure.com/",
      "Key": "<...>"
    },
    "DurableSystemPrompt": {
      "BlobStorageConnection": "<...>"
    },
    "CognitiveSearchMemorySource": {
      "Endpoint": "https://<...>-cog-search.search.windows.net",
      "Key": "<...>",
      "ConfigBlobStorageConnection": "<...>"
    },
    "BlobStorageMemorySource": {
      "ConfigBlobStorageConnection": "<...>"
    }
  }
}
```
