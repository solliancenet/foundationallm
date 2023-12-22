# FoundationaLLM Gatekeepr API

The Gatekeeper API facilities requests from the Core API or other external applications.

## Overview

The Gatekeeper API is secured via an `X-API-KEY` and requires a user context that is typically passed from the Azure Entra/Identity platform secured UI/application.

The Gatekeeper API provides the following services:

- Handles Completion and Summary requests from the Core API
- Calls the Azure Content Safety API to filter improper user prompts
- Sends Completion and Summary requests to Agent Factory API

## Instructions

Coming soon.

## Troubleshooting

### Service is not starting

Ensure the environment variable is set:

- FoundationaLLM:AppConfig:ConnectionString

Ensure that all configuration values have been set in the Azure Key Vault along with the corresponding App Configuration settings. These include:

- FoundationaLLM:APIs:GatekeeperAPI:AppInsightsConnectionString
- FoundationaLLM:APIs:{HttpClients.GatekeeperAPI}
- FoundationaLLM:AzureContentSafety:APIUrl
- FoundationaLLM:AzureContentSafety:APIKey (foundationallm-azurecontentsafety-apikey)
- FoundationaLLM:APIs:{HttpClients.AgentFactoryAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.AgentFactoryAPI}:APIKey (foundationallm-apis-agentfactoryapi-apikey)
