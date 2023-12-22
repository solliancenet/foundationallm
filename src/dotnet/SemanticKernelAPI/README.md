# FoundationaLLM Semantic Kernel API

The Semantic Kernel API facilities requests from the Gatekeeper API or other external applications.

## Overview

The Semantic Kernel API is secured via Azure AD and an `X-API-KEY` and requires a user context that is typically passed from the Azure AD/Identity platform secured UI/application.

The Semantic Kernel API provides the following services:

- Handle user prompts for completion and summaries from the Agent Factory API.
- Executes the defined skills in the Semantic Kernel configurations to generate completions
- Handles the various memories required to execute the AI layer summaries

## Instructions

Coming soon.

## Services Required

Coming soon.

## Troubleshooting

### Service is not starting

Ensure the environment variable is set:

- FoundationaLLM:AppConfig:ConnectionString

Ensure that all configuration values have been set in the App Configuration/Azure Key Vault. These include the following with App Config name mapped to Azure Key Vault name (if applicable):

- FoundationaLLM:APIs:SemanticKernelAPI:AppInsightsConnectionString
- FoundationaLLM:APIs:{HttpClients.SemanticKernelAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.SemanticKernelAPI}:APIKey(foundationallm-apis-sementickernalapi-apikey)
- FoundationaLLM:DurableSystemPrompt
- FoundationaLLM:CognitiveSearchMemorySource
- FoundationaLLM:BlobStorageMemorySource
