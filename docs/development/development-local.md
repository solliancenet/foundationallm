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
      "APIUrl": "",
      "APIKey": ""
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
      "APIUrl": "<...>",
      "APIKey": "<...>"
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
      "APIKeyPath": "FoundationaLLM:GatekeeperAPI:APIKey"
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
      "APIKeyPath": "FoundationaLLM:AgentFactoryAPI:APIKey"
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

## Agent Hub API

## Data Source Hub API

## Prompt Hub API

## LangChain API

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
      "APIKeyPath": "FoundationaLLM:SemanticKernelOrchestration:APIKey"
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
