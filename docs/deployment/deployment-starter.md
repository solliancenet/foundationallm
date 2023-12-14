# Deployment - Starter

Foundationa**LLM** deploys into your own Azure Subscription. By default it will deploy to Azure Container Apps (ACA) that make it fast to get started. When you want to deploy to production at scale, you can also deploy to Azure Kubernetes Service (AKS). Given that there are Azure Subscription quota limits to the number of Azure OpenAI Service resources you can deploy, you can choose to use an existing Azure OpenAI Service resource instead of a creating a new one with your deployment.

## Prerequisites

- Azure Subscription (Subscription needs to be whitelisted for Azure OpenAI).
- Subscription access to Azure OpenAI service. Start here to [Request Access to Azure OpenAI Service](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xUNTZBNzRKNlVQSFhZMU9aV09EVzYxWFdORCQlQCN0PWcu).
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
- Docker Desktop.
- Azure CLI ([v2.51.0 or greater](https://docs.microsoft.com/cli/azure/install-azure-cli)).
- Helm ([v3.11.1 or greater](https://helm.sh/docs/intro/install/)).
- Visual Studio 2022 (only needed if you plan to run/debug the solution locally).
- Minimum quota of 65 CPUs across all VM family types. Start here to [Manage VM Quotas](https://learn.microsoft.com/azure/quotas/per-vm-quota-requests).
- Two App Registrations created in the Entra ID tenant (Azure Active Directory).
- User with the following role assignments:
    - Owner on the target subscription;
    - Owner on the two app registrations.

## Deployment steps

Follow the steps below to deploy the solution to your Azure subscription. You will be prompted to log in to your Azure account during the deployment process.

1. Ensure all the prerequisites are met, and that Docker Desktop is running.  

1. From a PowerShell prompt, execute the following to clone the repository:

    ```cmd
    git clone https://github.com/solliancenet/foundationallm.git
    ```

1. Open a PowerShell instance and run the following script to provision the infrastructure and deploy the API and frontend. This will provision all of the required infrastructure, deploy the API and web app services, and import data into Cosmos DB.

    1. Option 1: Full deployment using Microsoft Azure Container Apps (ACA)

        ```pwsh
        cd foundationallm
        ./deploy/scripts/Unified-Deploy.ps1 -resourceGroup <rg_name> -location <location> -subscription <target_subscription_id>
        ```

        >**NOTE**: Make sure to set the `<location>` value to a region that supports Azure OpenAI services.  See [Azure OpenAI service regions](https://azure.microsoft.com/explore/global-infrastructure/products-by-region/?products=cognitive-services&regions=all) for more information.

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

## Post-deployment configuration

### Microsoft Entra ID authentication setup

Follow the instructions in the [Microsoft Entra ID setup document](./authentication-setup-entra.md) to configure Microsoft Entra ID authentication for the solution.

## Update individual APIs or portals

### Update individual APIs or portals when using Microsoft Azure Container Apps (ACA)

To update an individual API or portal, you can use the following commands:

1. Login with your Entra ID account:
   
    ```pwsh
    az login
    ```
2. Set the target subscription:
   
    ```pwsh
    az account set --subscription <target_subscription_id>
    ```

3. Navigate to the root folder of the repository, and checkout the branch you want to deploy:
   
    ```pwsh
    cd <path_to_repository> 
    git checkout <branch_name>
    git pull
    ```

4. Update the `docker-compose.yml` file located in `deploy\docker` so that it lists only the images you are planning to update.
5. Build and push the Docker images to the Azure Container Registry (ACR) using the following command (`BuildPush.ps1` is located in the  `scripts` folder):
   
    ```pwsh
    .\BuildPush.ps1 -resourceGroup <resource_group_name> -acrName <acr_name>
    ```
   where `<resource_group_name>` is the name of the resource group where the Azure Container Registry is located, and `<acr_name>` is the name of the Azure Container Registry.

6. For each ACA you want to update, run the following script:

    ```pwsh
    az containerapp update --name <aca_name> --resource-group <resource_group_name> --image <image_name>
    ```
    where `<aca_name>` is the name of the ACA, `<resource_group_name>` is the name of the resource group where the ACA is located, and `<image_name>` is the name of the image you want to update (the structure of the image name is `<acr_name>.azurecr.io/<image_name>:latest`, where `<acr_name>` is the name of the Azure Container Registry, and `<image_name>` is the name of the Docker image).

