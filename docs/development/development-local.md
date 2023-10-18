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

Environment variable needs to be set for Application Configuration Service URL. This environment variable needs to be named `FoundationaLLM:AppConfig:ConnectionString`.

## Chat

### Chat app settings

> Make sure the contents of the `appsettings.json` file has this structure and similar values:

```json
{
  "DetailedErrors": true,
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "FoundationaLLM": {
    "AppConfig": {
      "ConnectionString": ""
    }
  }
}
```

> Create the `appsettings.Development.json` file or update it with the following content and replace all `<...>` placeholders with the values from your deployment:

```json
{
  "FoundationaLLM": {
    "APIs": {
      "CoreAPI": {
        "APIUrl": "<...>"
      },
    } 
  }
 }

```

## Core API

### Core API app settings

> Make sure the contents of the `appsettings.json` file has this structure and similar values:

```json
{
  "DetailedErrors": true,
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "FoundationaLLM": {
    "AppConfig": {
      "ConnectionString": ""
    }
  }
}
```

> Create the `appsettings.Development.json` file or update it with the following content and replace all `<...>` placeholders with the values from your deployment:

```json
{  
  "APIs": {
    "GatekeeperAPI": {
      "APIUrl": "<...>"
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
    "AppConfig": {
      "ConnectionString": ""
    }
  }
}
```

> Create the `appsettings.Development.json` file or update it with the following content and replace all `<...>` placeholders with the values from your deployment:

```json
{
  "FoundationaLLM": {
    "APIs": {
      "AgentFactoryAPI": {
        "APIUrl": "<...>"
      },
    
      "GatekeeperAPI": {
        "APIUrl": "<...>"
      }
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
    "AppConfig": {
      "ConnectionString": ""
    }
  }
}
```

> Create the `appsettings.Development.json` file or update it with the following content and replace all `<...>` placeholders with the values from your deployment:

```json
{
  "FoundationaLLM": {
    "APIs": {
      "AgentFactoryAPI": {
        "APIUrl": "<...>"
      },
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
    }
  }
}
```

## PythonSDK

### PythonSDK Environment Variables

The PythonSDK has two different run options. One, to use environment variables, and another to access all settings from the Azure App Configuration service. This behavior is controlled via the environment variable `foundationallm-configuration-allow-environment-variables`, set to True if using environment variabes, False if using Azure App Configuration.

The local environment also requires the environment variable `foundationallm-app-configuration-uri`.

If `foundationallm-configuration-allow-environment-variables` is true, and the value being requested is not found, it will back off to the Azure App Configuration service.

Mandatory environment variables:

| Name | Value | Description |
| ---- | ----- | ----------- |
| foundationallm-configuration-allow-environment-variables | True | Allow environment variables |
| foundationallm-app-configuration-uri | REDACTED | Azure App Configuration URI |

Environment variables required if `foundationallm-configuration-allow-environment-variables` is set to `True`:

| Name | Value | Description |
| ---- | ----- | ----------- |
| FoundationaLLM:AgentHub:StorageManager:BlobStorage:ConnectionString | REDACTED | Storage account that contains the Agent metadata container |
| FoundationaLLM:AgentHub:AgentMetadata:StorageContainer | agent | Agent metadata storage container name |
| FoundationaLLM:DataSourceHub:StorageManager:BlobStorage:ConnectionString | REDACTED | Storage account that contains the DataSource metadata container |
| FoundationaLLM:DataSourceHub:DataSourceMetadata:StorageContainer | data-sources | Datasource metadata storage container name |
| FoundationaLLM:PromptHub:StorageManager:BlobStorage:ConnectionString | REDACTED | Storage account that contains the Prompt metadata container |
| FoundationaLLM:PromptHub:PromptMetadata:StorageContainer | system-prompt | Prompt metadata storage container name |
| FoundationaLLM:AzureOpenAI:API:Endpoint | REDACTED | Azure Open AI instance URL |
| FoundationaLLM:AzureOpenAI:API:Key | REDACTED | Azure Open AI instance key |
| FoundationaLLM:AzureOpenAI:API:Version | 2023-07-01-preview | Azure Open AI API version |
| FoundationaLLM:AzureOpenAI:API:Completions:DeploymentName | completions | Azure Open AI completions deployment name |
| FoundationaLLM:AzureOpenAI:API:Completions:ModelVersion | 0301 | Azure Open AI completions model version |
| FoundationaLLM:AzureOpenAI:API:Completions:MaxTokens | 4097 | Azure Open AI completions max tokens |
| FoundationaLLM:AzureOpenAI:API:Completions:Temperature | 0 | Azure Open AI completions temperature |
| FoundationaLLM:LangChain:Summary:ModelName | gpt-35-turbo | LangChain summary model name |
| FoundationaLLM:LangChain:Summary:MaxTokens | 4097 | LangChain summary model max tokens |
| FoundationaLLM:OpenAI:API:Key | REDACTED | OpenAI API key |
| FoundationaLLM:OpenAI:API:Endpoint | REDACTED | OpenAI API endpoint |
| FoundationaLLM:OpenAI:API:Temperature | 0 | OpenAI API temperature |

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
