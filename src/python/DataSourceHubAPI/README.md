# FoundationaLLM Data Source Hub API

The Data Source Hub API facilities requests from the Gatekeeper API or other external applications.

## Overview

The Data Source Hub API is secured via Azure AD and an `X-API-KEY` and requires a user context that is typically passed from the Azure AD/Identity platform secured UI/application.

The Data Source Hub API provides the following services:

- Resolves a request from the Agent Factory to return a target Data source configuration
- Data source configuration will be retrieved from the backing data source (currently Azure Blob Storage)

## Instructions

Coming soon.

## Troubleshooting

### Service is not starting

Ensure the environment variable is set:

- FoundationaLLM:AppConfig:ConnectionString

Ensure that all configuration values have been set in the App Configuration/Azure Key Vault. These include the following with App Config name mapped to Azure Key Vault name (if applicable):

- FoundationaLLM:APIs:DataSourceHubAPI:Key (foundationallm-apis-datasourcehubapi-apikey)
- FoundationaLLM:DataSourceHub:StorageManager:BlobStorage:ConnectionString (foundationallm-datasourcehub-storagemanager-blobstorage-connectionstring)
- FoundationaLLM:DataSourceHub:AgentMetadata:StorageContainer

### Data Source Hub not returning agents

Ensure that you have loaded at least one data source configuration into the blog storage container and that the search criteria matches what is being requested from the Agent Factory API.
