# Foundationa**LLM**: A platform accelerating delivery of secure, trustworthy enterprise copilots

Foundationa**LLM** simplifies and streamlines building *knowledge management* (e.g., question/answer agents) and *analytic* (e.g., self-service business intelligence) copilots over the data sources present across your enterprise.  

Foundationa**LLM** deploys a comprehensive and highly configurable copilot platform to your Azure cloud environment:

- Simplifies integration with enterprise data sources used by agent for in-context learning (e.g., enabling RAG, CoT, ReAct and inner monologue patterns).
- Provides defense in depth with fine-grain security controls over data used by agent and pre/post completion filters that guard against attack.
- Hardened solution attacked by an LLM red team from inception.
- Scalable solution load balances across multiple LLM endpoints.
- Extensible to new data sources, new LLM orchestrators and LLMs.

## What do WE mean by "copilot"

It's a rapidly evolving AI world out there, so let's level set on what we mean when we say **copilot** as this is concept core to Foundationa**LLM**. 

At its most basic, a copilot uses enterprise supplied knowledge and generative AI models to author text, write code or render images, often by reasoning over human supplied prompts. Across these modalities, the AI is used to assist a human directly with a specific task. That's what makes it a copilot.    

This basic capability emerges in copilots which power these scenarios:

- Knowledge Management: Help users quickly find the information they seek and deliver at the right level and in the right format. Examples include summarization and sentiment analysis. 

- Analytics: Help users quickly get to the data driven insights they seek. Examples include recommendations, predictions, anomaly detection, statistical analysis and data querying and reporting.


## Why is Foundationa**LLM** Needed?

Simply put we saw lot of folks reinventing the wheel just to get a customized copilot that was grounded and bases its responses in their own data as opposed to the trained parametric knowledge of the model. Many of the solutions we saw made for great demos, but were effectively toys wrapping calls to OpenAI endpoints- they were not something intended or ready to take into production. We built Foundationa**LLM** to provide a continous journey, one that was quick to get started with so folks could experiment quickly with LLM's but not fall off a cliff after that with a solution that would be insecure, unlicensed, inflexible and not fully featured enough to grow from the prototype into a production solution without having to start all over.  

The core problems to deliver enterprise copilots are:
- Enterprise grade copilots are complex and have lots of moving parts (not to mention infrastructure).
- The industry has a skills gap when it comes to filling the roles needed to deliver these complex copilot solutions.
- The top AI risks (inaccuracy, cybersecurity, compliance, explainability, privacy) are not being mitigated. 
- Delivery of a copilot solution is time consuming, expensive and frustrating. 

## Getting Started

FoundationalLLM provides a simple command line driven approach to getting your first deployment up and running. Basically, it's two commands. After that, you can customize the solution, run it locally on your machine and update the deployment with your customizations.

Follow the [Standard Deployment instructions](./docs/deployment/deployment-standard.md) to get Foundationa**LLM** deployed in your Azure subscription.


### Prerequisites

- Azure Subscription
- Subscription access to Azure OpenAI service. Start here to [Request Access to Azure OpenAI Service](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xUOFA5Qk1UWDRBMjg0WFhPMkIzTzhKQ1dWNyQlQCN0PWcu)
- .NET 7 SDK
- Docker Desktop
- Azure CLI ([v2.49.0 or greater](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli))
- Helm ([v3.11.1 or greater](https://helm.sh/))
- Visual Studio 2022 (only needed if you plan to run/debug the solution locally)

### Deployment

Clone the Foundationa**LLM** repository

```pwsh
git clone https://github.com/solliancenet/foundationallm
```

### Initial Deployment

Run the following PowerShell script to provision the infrastructure and deploy the API and frontend. This will provision all of the required infrastructure, deploy the API and web app services into AKS, and import starter data into Cosmos.

```pwsh
./deploy/scripts/Unified-Deploy.ps1 -resourceGroup <resource-group-name> `
                                    -location <location> `
                                    -subscription <subscription-id>
```

>**NOTE**: Make sure to set the `<location>` value to a region that supports Azure OpenAI services.  See [Azure OpenAI service regions](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=cognitive-services&regions=all) for more information.

#### Deployments using an existing OpenAI service

By default, Foundationa**LLM** will create a new Azure OpenAI instance during deployment. If you need to use an existing OpenAI service, run the following from the `./deploy/scripts`.  This will provision all of the necessary infrastruction except the Azure OpenAI service and will deploy the API and frontend to an AKS cluster via Helm.

```pwsh
./Unified-Deploy.ps1 -resourceGroup <resource-group-name> `
                     -location <location> `
                     -subscription <subscription-id> `
                     -openAiName <openAi-service-name> `
                     -openAiRg <openAi-resource-group-name> `
                     -openAiCompletionsDeployment <openAi-completions-deployment-name> `
                     -openAiEmbeddingsDeployment <openAi-embeddings-deployment-name>
```
The default deployment should take about **10 minutes**. After this deployment completes you can continue with the next section. So, maybe go get a quick cup of coffee or stretch in before you get to the fun stuff?

### After deployment

1. Navigate to resource group and obtain the name of the AKS service and execute the following command to obtain the OpenAI Chat endpoint

  ```pwsh
  az aks show -n <aks-name> -g <resource-group-name> -o tsv --query addonProfiles.httpApplicationRouting.config.HTTPApplicationRoutingZoneName
  ```

1. Browse to the web app with the returned hostname.
1. Click [+ Create New Chat] button to create a new chat session.
1. Type in your questions in the text box and press Enter.

Here are some sample questions you can ask:

- Who are you?
- TBD


## Run locally and debug

This solution can be run locally post deployment. Below are the steps.

### Local steps
Use the steps that follow to run the solution on your local machine.
#### Configure local settings

- In the `FoundationaLLM.Chat` project, make sure the content of the `appsettings.json` file is similar to this:

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
        "MSCosmosDBOpenAI": {
            "ChatManager": {
                "APIUrl": "https://localhost:63279",
                "APIRoutePrefix": ""
            }
        }
    }
    ```

- In the `FoundationaLLM.ChatAPI` project, make sure the content of the `appsettings.json` file is similar to this:

    ```json
    {
        "Logging": {
            "LogLevel": {
                "Default": "Information",
                "Microsoft.AspNetCore": "Warning"
            }
        },
        "AllowedHosts": "*",
        "MSCosmosDBOpenAI": {
            "CognitiveSearch": {
                "IndexName": "vector-index",
                "MaxVectorSearchResults": 10
            },
            "OpenAI": {
                "CompletionsDeployment": "completions",
                "CompletionsDeploymentMaxTokens": 4096,
                "EmbeddingsDeployment": "embeddings",
                "EmbeddingsDeploymentMaxTokens": 8191,
                "ChatCompletionPromptName": "RetailAssistant.Default",
                "ShortSummaryPromptName": "Summarizer.TwoWords",
                "PromptOptimization": {
                    "CompletionsMinTokens": 50,
                    "CompletionsMaxTokens": 300,
                    "SystemMaxTokens": 1500,
                    "MemoryMinTokens": 500,
                    "MemoryMaxTokens": 2500,
                    "MessagesMinTokens": 1000,
                    "MessagesMaxTokens": 3000
                }
            },
            "CosmosDB": {
                "Containers": "completions, customer, product",
                "Database": "database",
                "ChangeFeedLeaseContainer": "leases"
            },
            "DurableSystemPrompt": {
                "BlobStorageContainer": "system-prompt"
            }
        }
    }
    ```

- In the `FoundationaLLM.ChatAPI` project, create an `appsettings.Development.json` file with the following content (replace all `<...>` placeholders with the values from your deployment):

    ```json
    {
        "MSCosmosDBOpenAI": {
            "CognitiveSearch": {
                "Endpoint": "https://<...>.search.windows.net",
                "Key": "<...>"
            },
            "OpenAI": {
                "Endpoint": "https://<...>.openai.azure.com/",
                "Key": "<...>"
            },
            "CosmosDB": {
                "Endpoint": "https://<...>.documents.azure.com:443/",
                "Key": "<...>"
            },
            "DurableSystemPrompt": {
                "BlobStorageConnection": "<...>"
            }
        }
    }
    ```

    >**NOTE**: THe `BlobStorageConnection` value can be found in the Azure Portal by navigating to the Storage Account created by the deployment (the one that has a container named `system-prompt`) and selecting the `Access keys` blade. The value is the `Connection string` for the `key1` key.

#### Using Visual Studio

To run locally and debug using Visual Studio, open the solution file to load the projects and prepare for debugging.

Before you can start debugging, you need to set the startup projects. To do this, right-click on the solution in the Solution Explorer and select `Set Startup Projects...`. In the dialog that opens, select `Multiple startup projects` and set the `Action` for the `ChatServiceWebApi` and `Search` projects to `Start`.

Also, make sure the newly created `appsettings.Development.json` file is copied to the output directory. To do this, right-click on the file in the Solution Explorer and select `Properties`. In the properties window, set the `Copy to Output Directory` property to `Copy always`..

You are now ready to start debugging the solution locally. To do this, press `F5` or select `Debug > Start Debugging` from the menu.

**NOTE**: With Visual Studio, you can also use alternate ways to manage the secrets and configuration. For example, you can use the `Manage User Secrets` option from the context menu of the `ChatWebServiceApi` project to open the `secrets.json` file and add the configuration values there.

### Uploading New Sample Data

To upload new data, or to extend the solution to ingest your own data, that will be processed by the change feed and then made available as context data for chat completions we recommend you use the [Cosmos DB Desktop Migration Tool](https://github.com/AzureCosmosDB/data-migration-desktop-tool) to copy your source data into the appropriate container within the deployed instance of Cosmos DB. 

Open a PowerShell and run the following lines to download and extract `dmt.exe`:
```ps
$dmtUrl="https://github.com/AzureCosmosDB/data-migration-desktop-tool/releases/download/2.1.1/dmt-2.1.1-win-x64.zip"
Invoke-WebRequest -Uri $dmtUrl -OutFile dmt.zip
Expand-Archive -Path dmt.zip -DestinationPath .
```

In the folder containing the extracted files, you will see a `migrationsettings.json` file. You will need to edit this file and provide the configuration for the source (e.g., your local files), and the sink (e.g., a container in Cosmos DB).

Here is an example migrationsettings file setup to load a local JSON file, stored in a data folder, to a container in Cosmos DB. Edit this file to suit your needs and save it.

```json
{
  "Source": "JSON",
  "Sink": "Cosmos-nosql",
  "Operations": [
    {
      "SourceSettings": {
        "FilePath": "data\\sampleData.json"
      },
      "SinkSettings": {
        "ConnectionString": "AccountEndpoint=YOUR_CONNECTION_STRING_HERE",
        "Database":"database",
        "Container":"raw",
        "PartitionKeyPath":"/id",
        "RecreateContainer": false,
        "BatchSize": 100,
        "ConnectionMode": "Direct",
        "MaxRetryCount": 5,
        "InitialRetryDurationMs": 200,
        "CreatedContainerMaxThroughput": 1000,
        "UseAutoscaleForCreatedContainer": true,
        "WriteMode": "InsertStream",
        "IsServerlessAccount": false
        }
    }
  ]
}
```

Then run the tool with the following command.

```ps
.\dmt.exe
```

If no errors appear, your new data should now be available in the configured container.

NOTE: If you want to build a reusable, automated script to deploy your files, take a look at the `./deploy/scripts/Import-Data.ps1` in the source code of this project.

## Clean-up

Delete the resource group to delete all Azure resources deployed by Foundationa**LLM**.

