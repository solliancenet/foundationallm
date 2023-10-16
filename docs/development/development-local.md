# Configure local development environment

- [Configure local development environment](#configure-local-development-environment)
  - [Requirements](#requirements)
  - [Chat](#chat)
    - [Chat app settings](#chat-app-settings)
  - [Core API](#core-api)
    - [Core API app settings](#core-api-app-settings)
  - [Gatekeeper API](#gatekeeper-api)
    - [Gatekeeper API app settings](#gatekeeper-api-app-settings)
  - [Agent Factory API](#agent-factory-api)
    - [Agent Factory API app settings](#agent-factory-api-app-settings)
  - [PythonSDK](#pythonsdk)
    - [PythonSDK Environment Variables](#pythonsdk-environment-variables)
  - [Agent Hub API](#agent-hub-api)
    - [Agent Hub API Environment Variables](#agent-hub-api-environment-variables)
  - [Data Source Hub API](#data-source-hub-api)
    - [Data Source Hub API Environment Variables](#data-source-hub-api-environment-variables)
  - [Prompt Hub API](#prompt-hub-api)
    - [Prompt Hub API Environment Variables](#prompt-hub-api-environment-variables)
  - [LangChain API](#langchain-api)
    - [LangChain API Environment Variables](#langchain-api-environment-variables)
  - [Semantic Kernel API](#semantic-kernel-api)
    - [Semantic Kernel API app settings](#semantic-kernel-api-app-settings)

## Requirements

## Chat

### Chat app settings

> Make sure the contents of the `appsettings.json` file has this structure and similar values:

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

> Create the `appsettings.Development.json` file or update it with the following content and replace all `<...>` placeholders with the values from your deployment:

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

### Core API app settings

> Make sure the contents of the `appsettings.json` file has this structure and similar values:

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
    "DownstreamAPIs": {    
      "GatekeeperAPI": {
        "APIUrl": "",
        "APIKeySecretName": "foundationallm-gatekeeper-api-key"
      }
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

> Create the `appsettings.Development.json` file or update it with the following content and replace all `<...>` placeholders with the values from your deployment:

```json
{
  "FoundationaLLM": {
    "CosmosDB": {
      "Endpoint": "https://<...>-cosmos-nosql.documents.azure.com:443/",
      "Key": "<...>"
    },
    "DownstreamAPIs": {
      "GatekeeperAPI": {
        "APIUrl": "<...>"
      }
    },
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

## Gatekeeper API

### Gatekeeper API app settings

> Make sure the contents of the `appsettings.json` file has this structure and similar values:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning"
      }
    }
  },
  "AllowedHosts": "*",
  "FoundationaLLM": {
    "DownstreamAPIs": {
      "AgentFactoryAPI": {
        "APIUrl": ""
      }
    },
    "GatekeeperAPI": {
      "APIKeySecretName": "foundationallm-gatekeeper-api-key"
    },
    "AzureContentSafety": {
      "HateSeverity": 2,
      "ViolenceSeverity": 2,
      "SelfHarmSeverity": 2,
      "SexualSeverity": 2,
      "APIKeySecretName": "foundationallm-content-safety-key"
    }
  }
}
```

> Create the `appsettings.Development.json` file or update it with the following content and replace all `<...>` placeholders with the values from your deployment:

```json
{
  "FoundationaLLM": {
    "DownstreamAPIs": {
      "AgentFactoryAPI": {
        "APIUrl": "<...>"
      }
    },
    "AzureContentSafety": {
      "Endpoint": "<...>"
    }
  }
}
```

## Agent Factory API

### Agent Factory API app settings

> Make sure the contents of the `appsettings.json` file has this structure and similar values:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning"
      }
    }
  },
  "AllowedHosts": "*",
  "FoundationaLLM": {
      "AgentFactory": {
        "DefaultOrchestrationService": "LangChain"
      },
      "DownstreamAPIs": {
          "LangChainAPI": {
            "APIUrl": "",
            "APIKeySecretName": "foundationallm-langchain-api-key"
          },
          "SemanticKernelAPI": {
            "APIUrl": "",
            "APIKeySecretName": "foundationallm-semantic-kernel-orchestration-api-key"
          },
          "AgentHubAPI": {
            "APIUrl": "",
            "APIKeySecretName": "foundationallm-agenthub-api-key"
          },
          "PromptHubAPI": {
            "APIUrl": "",
            "APIKeySecretName": "foundationallm-prompthub-api-key"
          },
          "DataSourceHubAPI": {
            "APIUrl": "",
            "APIKeySecretName": "foundationallm-datasourcehub-api-key"
          }
      },
    "AgentFactoryAPI": {
      "APIKeySecretName": "foundationallm-agent-factory-api-key"
    },
    "Configuration": {
      "KeyVaultUri": ""
    }
  }
}
```

> Create the `appsettings.Development.json` file or update it with the following content and replace all `<...>` placeholders with the values from your deployment:

```json
{
  "FoundationaLLM": {
    "DownstreamAPIs": {
      "LangChainAPI": {
        "APIUrl": "<...>"
      },
      "SemanticKernelAPI": {
        "APIUrl": "<...>"
      },
      "AgentHubAPI": {
        "APIUrl": "<...>"
      },
      "PromptHubAPI": {
        "APIUrl": "<...>"
      },
      "DataSourceHubAPI": {
        "APIUrl": "<...>"
      }
    },  
    "Configuration": {
      "KeyVaultUri": "<...>"
    }
  }
}
```

## PythonSDK

### PythonSDK Environment Variables

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

### Agent Hub API Environment Variables

| Name | Value | Description |
| ---- | ----- | ----------- |

## Data Source Hub API

### Data Source Hub API Environment Variables

| Name | Value | Description |
| ---- | ----- | ----------- |

## Prompt Hub API

### Prompt Hub API Environment Variables

| Name | Value | Description |
| ---- | ----- | ----------- |

## LangChain API

### LangChain API Environment Variables

| Name | Value | Description |
| ---- | ----- | ----------- |

## Semantic Kernel API

### Semantic Kernel API app settings

> Make sure the contents of the `appsettings.json` file has this structure and similar values:

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

> Create the `appsettings.Development.json` file or update it with the following content and replace all `<...>` placeholders with the values from your deployment:

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
