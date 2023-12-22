# FoundationaLLM Agent Factory API

The Agent Factory API facilities requests from the Gatekeeper API or other external applications.

## Overview

The Agent Factory API is secured via an `X-API-KEY` and requires a user context that is typically passed from the Azure Entra/Identity platform secured UI/application.

The Agent Factory API provides the following services:

- Handles Completion and Summary requests from the Gatekeeper API
- Determines the best agent or agents for a user prompt
- Based on agent configuration, the agent factory will call the specific orchestrator (Semantic Kernel or LangChain)
- Builds a target(s) agent for the user prompt
  - Looks for a proper prompt to send to the sub-agents
  - Loads data sources for target agents (Data Source API)
- Performs IAM against requested entities (Agents, Data Sources, etc) based on user context
- Proxies the `resolve` requests for a user prompt and returns the completions to the Gatekeeper API
  
Downstream services that are called include:

- Agent Hub API
- Data Source API
- LangChain API
- Prompt Hub API

## Instructions

Coming soon.

## Troubleshooting

### Service is not starting

Ensure the environment variable is set:

- FoundationaLLM:AppConfig:ConnectionString

Ensure that all configuration values have been set in the App Configuration/Azure Key Vault. These include the following with App Config name mapped to Azure Key Vault name (if applicable):

- FoundationaLLM:APIs:AgentFactoryAPI:AppInsightsConnectionString (foundationallm-apis-agentfactoryapi-apikey)
- FoundationaLLM:APIs:{HttpClients.AgentFactoryAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.AgentFactoryAPI}:APIKey (foundationallm-apis-agentfactoryapi-apikey)
- FoundationaLLM:APIs:{HttpClients.SemanticKernelAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.SemanticKernelAPI}:APIKey (foundationallm-apis-semantickernalapi-apikey)
- FoundationaLLM:APIs:{HttpClients.LangChainAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.LangChainAPI}:APIKey (foundationallm-apis-langchainapi-apikey)
- FoundationaLLM:APIs:{HttpClients.AgentHubAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.AgentHubAPI}:APIKey (foundationallm-apis-agenthubapi-apikey)
- FoundationaLLM:APIs:{HttpClients.PromptHubAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.PromptHubAPI}:APIKey (foundationallm-apis-prompthubapi-apikey)
- FoundationaLLM:AgentFactory

> NOTE: The APIUrl and APIKey (and most other values) are configured automatically for you via the deployment process, however, if the endpoints change due to some post configuration change, you will need to validate the urls and keys are still valid.
