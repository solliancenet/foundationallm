# FoundationaLLM Agent Hub API

The Agent Hub API facilities requests from the Gatekeeper API or other external applications.

## Overview

The Agent Hub API is secured via Azure AD and an `X-API-KEY` and requires a user context that is typically passed from the Azure AD/Identity platform secured UI/application.

The Agent Hub API provides the following services:

- Resolves a request from the Agent Factory to return a target Agent configuration
- Agent configuration will be retrieved from the backing data source (currently Azure Blob Storage)

## Instructions

Coming soon.

## Troubleshooting

### Service is not starting

Ensure the environment variable is set:

- FoundationaLLM:AppConfig:ConnectionString

Ensure that all configuration values have been set in the App Configuration/Azure Key Vault. These include the following with App Config name mapped to Azure Key Vault name (if applicable):

- FoundationaLLM:APIs:AgentHubAPI:Key (foundationallm-apis-agenthubapi-apikey)
- FoundationaLLM:AgentHub:StorageManager:BlobStorage:ConnectionString (foundationallm-agenthub-storagemanager-blobstorage-connectionstring)
- FoundationaLLM:AgentHub:AgentMetadata:StorageContainer

### Agent Hub not returning agents

Ensure that you have loaded at least one agent configuration into the blog storage container and that the search criteria matches what is being requested from the Agent Factory API.
