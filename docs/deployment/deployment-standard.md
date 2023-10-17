# Deployment - Standard

Foundationa**LLM** deploys into your own Azure Subscription. By default it will deploy to Azure Container Apps (ACA) that make it fast to get started. When you want to deploy to production at scale, you can also deploy to Azure Kubernetes Service (AKS). Given that there are Azure Subscription quota limits to the number of Azure OpenAI Service resources you can deploy, you can choose to use an existing Azure OpenAI Service resource instead of a creating a new one with your deployment.

## Prerequisites

- Azure Subscription
- Subscription access to Azure OpenAI service. Start here to [Request Access to Azure OpenAI Service](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xUOFA5Qk1UWDRBMjg0WFhPMkIzTzhKQ1dWNyQlQCN0PWcu)
- .NET 7 SDK
- Docker Desktop
- Azure CLI ([v2.51.0 or greater](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli))
- [Helm 3.11.1 or greater](https://helm.sh/docs/intro/install/)
- Visual Studio 2022 (only needed if you plan to run/debug the solution locally)

## Deployment steps

Follow the steps below to deploy the solution to your Azure subscription. You will be prompted to log in to your Azure account during the deployment process.

1. Ensure all the prerequisites are met, and that Docker Desktop is running.  

1. From a PowerShell prompt, execute the following to clone the repository:

    ```cmd
    git clone https://github.com/solliancenet/foundationallm.git
    ```

1. Run the following script to provision the infrastructure and deploy the API and frontend. This will provision all of the required infrastructure, deploy the API and web app services, and import data into Cosmos DB.

    1. Option 1: Full deployment using Microsoft Azure Container Apps (ACA)

        ```pwsh
        cd foundationallm
        ./deploy/scripts/Unified-Deploy.ps1 -resourceGroup <rg_name> -location <location> -subscription <target_subscription_id>
        ```

    2. Option 2: Full deployment using Microsoft Azure Kubernetes Service (AKS)
        To deploy to an AKS environment instead, run the same script with the added argument `-deployAks 1`, as shown below.  This will provision all of the required infrastructure, deploy the API and web app services as pods in an AKS cluster, and import data into Cosmos DB.

        ```pwsh
        cd foundationallm
        ./deploy/scripts/Unified-Deploy.ps1 -resourceGroup <rg_name> -location <location> -subscription <target_subscription_id> -deployAks 1
        ```

    3. Option 3: Deployment using an existing Azure OpenAI resource
        To deploy using an already provisioned Azure OpenAI resource, add the following parameters:
        | Parameter | Description |
        | --- | --- |
        | `-openAiRg` | The name of the resource group containing the Azure OpenAI resource. |
        | `-openAiName` | The name of the Azure OpenAI resource. |
        | `-openAiCompletionsDeployment` | The name given to the deployment of a completions model deployed within the Azure OpenAI resource, eg. `completions` |
        | `-openAiEmbeddingsDeployment` | The name given to the deployment of an embeddings model deployed within the Azure OpenAI resource, eg. `embeddings` |

        ACA:

        ```pwsh
        cd foundationallm
        ./deploy/scripts/Unified-Deploy.ps1 -resourceGroup <rg_name> -location <location> -subscription <target_subscription_id> -openAiRg <openai_rg_name> -openAiName <openai_resource_name> -openAiCompletionsDeployment <completions_deployment_name> -openAiEmbeddingsDeployment <embeddings_deployment_name>
        ```

        AKS:

        ```pwsh
        cd foundationallm
        ./deploy/scripts/Unified-Deploy.ps1 -resourceGroup <rg_name> -location <location> -subscription <target_subscription_id> -deployAks 1 -openAiRg <openai_rg_name> -openAiName <openai_resource_name> -openAiCompletionsDeployment <completions_deployment_name> -openAiEmbeddingsDeployment <embeddings_deployment_name>
        ```

>**NOTE**: Make sure to set the `<location>` value to a region that supports Azure OpenAI services.  See [Azure OpenAI service regions](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=cognitive-services&regions=all) for more information.
