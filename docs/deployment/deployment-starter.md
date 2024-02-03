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

### Authentication setup

Follow the instructions in the [Authentication setup document](authentication/index.md) to configure authentication for the solution.

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

3. Navigate to the FoundationaLLM GitHub Container Registry and obtain the SHA or image tag of the container you would like to update.

    ![Latest release of the image on the GitHub Container Registry.](./media/latest-image-release.png "Verifying Latest Image Release")

4. Use the following Azure CLI command to update the desired container. `--image` is a fully-qualified name (e.g., `ghcr.io/solliancenet/foundationallm/agent-factory-api:latest`).

    ```pwsh
    az containerapp update --name <aca_name> --resource-group <resource_group_name> --image <image_name>
    ```

    The following table indicates the mapping between each component of FLLM and the relevant Azure Container Apps instance (`--name`).

    | API | Container Name |
    | --- | -------------- |
    | Core API | `[PREFIX]coreca` |
    | Agent Factory API | `[PREFIX]agentfactoryca` |
    | Agent Hub API | `[PREFIX]agenthubca` |
    | Chat UI | `[PREFIX]chatuica` |
    | Core Job API | `[PREFIX]corejobca` |
    | Data Source Hub API | `[PREFIX]datasourcehubca` |
    | Gatekeeper API | `[PREFIX]gatekeeperca` |
    | Gatekeeper Integration API | `[PREFIX]gatekeeperintca` |
    | LangChain | `[PREFIX]langchainca` |
    | Management API | `[PREFIX]managementca` |
    | Management UI | `[PREFIX]managementuica` |
    | Prompt Hub API | `[PREFIX]prompthubca` |
    | Semantic Kernel API | `[PREFIX]semantickernelca` |
    | Vectorization API | `[PREFIX]vectorizationca` |
    | Vectorization Worker | `[PREFIX]vectorizationjobca` |

5. Alternatively, the `Update-Images-ACA-Starter.ps1` PowerShell script in the `deploy/scripts/` directory will update all containers in the deployment. Because it uses the Azure CLI, you must complete Steps 1-3.

   | Name | Flag | Optional | Default Value |
   | ---- | ---- | -------- | ------------- |
   | Deployment Resource Group | `-resourceGroup` | False | N/A |
   | Deployment Prefix | `-resourcePrefix` | False | N/A |
   | Deployment Subscription | `-subscription` | False | N/A |
   | Image Repository | `-containerPrefix` | True | `solliancenet/foundationallm` |
   | Image Tag | `-containerTag` | True | `latest` |
   | Registry | `-registry` | True | `ghcr.io` |

   ```powershell
   ./Update-Images-ACA-Starter.ps1 `
      -resourceGroup "[DEPLOYMENT RESOURCE GROUP]" `
      -resourcePrefix "[DEPLOYMENT PREFIX]" `
      -subscription "[DEPLOYMENT SUBSCRIPTION]" `
      -containerTag "[IMAGE TAG]"
   ```
