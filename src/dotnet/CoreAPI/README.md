# FoundationaLLM Core API

The Core API facilities requests from the Chat UI or other external applications.  

## Overview

The Core API is secured via Azure AD and requires a user context that is typically passed from the Azure AD/Identity platform secured UI/application.

The Core API provides the following services:

- Support custom branding of the Chat UI
- Create and load user chat sessions
- Pass user prompts for completion and summaries to the Gatekeeper API

## Instructions

Coming soon.

## Troubleshooting

### Chat UI will not load

Ensure the environment variable is set:

FoundationaLLM:AppConfig:ConnectionString

Ensure that all configuration values have been set in the App Configuration/Azure Key Vault. These include the following with App Config name mapped to Azure Key Vault name (if applicable):

- FoundationaLLM:CosmosDB:Key (foundationallm-cosmosdb-key)
- FoundationaLLM:Branding
- FoundationaLLM:APIs:CoreAPI:AppInsightsConnectionString
- FoundationaLLM:APIs:{HttpClients.GatekeeperAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.GatekeeperAPI}:APIKey (foundationallm-apis-gatekeeperapi-apikey)
- FoundationaLLM:CoreAPI:Entra:ClientSecret (foundationallm-coreapi-entra-clientsecret)
- FoundationaLLM:CoreAPI:Entra:Instance
- FoundationaLLM:CoreAPI:Entra:TenantId
- FoundationaLLM:CoreAPI:Entra:ClientId
- FoundationaLLM:CoreAPI:Entra:CallbackPath
- FoundationaLLM:CoreAPI:Entra:Scopes

### Login is not working

The most common issue is the Azure Entra application client id and secret is not configured properly.  Verify your settings and restart the Chat UI.

### Chat sessions are not displaying

Ensure the Gatekeeper API service is configured, running and not erroring.
