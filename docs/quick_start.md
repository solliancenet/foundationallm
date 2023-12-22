# FoundationaLLM Quick Start

This document will provide the high level steps that will guide you in getting up and running quickly with the FoundationaLLM platform.

## Step 1: Azure Setup

A couple of [ARM templates](https://learn.microsoft.com/en-us/azure/azure-resource-manager/templates/overview) have been provided that will do the initial setup of several required resources needed to support the core application and apis.  There are currently two versions (but several more, including a highly secured verion, will be released in the coming weeks).  

One version will deploy to an [Azure Kubernetes Service (AKS)](https://azure.microsoft.com/en-us/products/kubernetes-service) and the other will deploy into [Azure Container Apps (ACA)](https://azure.microsoft.com/en-us/products/container-apps):

- [AKS ARM Template](https://github.com/solliancenet/foundationallm/blob/main/deploy/arm/azuredeploy.json)
- [ACA ARM Template](https://github.com/solliancenet/foundationallm/blob/main/deploy/arm/azureAcaDeploy.json)

Currently the ARM template is targeted at three regions due to the fact that [Azure Open AI](https://azure.microsoft.com/en-us/products/ai-services/openai-service) and some models are only supported in a [limited set of regions](https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models).

As of this writing you will need to [request access to Azure Open AI](https://aka.ms/oai/access) in order to [create an Azure Open AI resource](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/create-resource).  

The ARM templates do not currently create an Azure Open AI instance due to the limits described above.  As a follow on step, **you will need to create an instance or point the configuration to an existing instance**.  When creating an instance, you should be aware of the [Azure Open AI quoats and limits](https://learn.microsoft.com/en-us/azure/ai-services/openai/quotas-limits) to avoid deployment or runtime issues.

Due to the Azure Open AI limits and requirements, we have provided the [Unified-Deploy.ps1](https://github.com/solliancenet/foundationallm/blob/main/deploy/scripts/Unified-Deploy.ps1) script that allow you to input your existing instance into the ARM deployment.

For in-depth steps on how to deploy the FoundationaLLM platform, reference the [Deployment - Standard documentation](https://github.com/solliancenet/foundationallm/blob/main/docs/deployment/deployment-standard.md).

## Step 2: Configuration

After deploying the ARM template there are some extra steps to get the platform to a fully functioning state.  

Although we have tried to make things as seamless as possible via the ARM templates and scripts, until we publish other setup scripts, there will be some steps you will need to do in order to get the platform fully running.  

These include:

- [Create and configure an Azure Entra application](https://github.com/solliancenet/foundationallm/blob/main/docs/deployment/authentication-setup-entra.md)

> NOTE:  In the next couple weeks we will be publishing configuration validation scripts that will help you to ensure that you have configured everything properly and for future debugging/testing purposes.

> NOTE: When changing these values, they may not propogate to the nodes immediately.  You should also check to see if you are referencing specific versions from app config in the key vault.

## Step 3: Verify Services

Once you have verified your deployment and configurations, you will want to test the various services.  This can be done by utilizing the published Postman configurations.  The services you should verify include:

- [Chat UI](https://github.com/solliancenet/foundationallm/tree/main/src/ui/Chat)
- [Core API](https://github.com/solliancenet/foundationallm/tree/main/src/dotnet/CoreAPI)
- [Gatekeeper API](https://github.com/solliancenet/foundationallm/tree/main/src/dotnet/GatekeeperAPI)
- [Agent Factory API](https://github.com/solliancenet/foundationallm/tree/main/src/dotnet/AgentFactoryAPI)
- [Semantic Kernel API](https://github.com/solliancenet/foundationallm/tree/main/src/dotnet/SemanticKernelAPI)
- [Agent Hub API](https://github.com/solliancenet/foundationallm/tree/main/src/python/AgentHubAPI)
- [Prompt Hub API](https://github.com/solliancenet/foundationallm/tree/main/src/python/PromptHubAPI)
- [Data Source API](https://github.com/solliancenet/foundationallm/tree/main/src/python/DataSourceHubAPI)
- [LangChain API](https://github.com/solliancenet/foundationallm/tree/main/src/python/LangChainAPI)

When running locally, the following ports will typically be used but are subject to change:

- Chat UI : https://localhost:7258/
- Core API : https://localhost:63279/
- Gatekeeper API : https://localhost:7180/
- Agent Factory API : https://localhost:7324/
- Semantic Kernel API : https://localhost:7062/
- Agent Hub API : https://localhost:8742/
- Prompt Hub API : https://localhost:8642/
- Data Source Hub API : https://localhost:8842/
- LangChain Hub API : https://localhost:8765/

The APIs are secured via a 'X-API-KEY' header.  These values are generated during the ARM template deployment and automatically set in the Azure Key Vault via the deployment script.  As a follow on step, you will want to modify these values to something unique per service.

> NOTE: When running via AKS or ACS, the urls will be generated during deployment of the services to the container instances (ex https://abc.xyz.eastus.azurecontainerapps.io/ or https://abc.eastus.aksapp.io/).

## Step 4: Explore

Once you have the first three steps completed, you can explore the platform by sending in message via the Chat UI.  

> NOTE:  The Chat UI is by default designed to utilize an external identity provider (by default Azure Entra) and you will need to ensure you have followed the Azure Entra configuration step to do this.

As a follow on to the basic setup and configuration, we encourage you to explore how to add your own prompts, agents, and data sources.

## Licensing

Please review our [licensing and support details](https://github.com/solliancenet/foundationallm/blob/main/LICENSE).
