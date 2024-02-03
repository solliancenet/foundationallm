# Updating container versions in the Standard Deployment

The Standard Deployment uses the `foundationallm/deploy/scripts/Deploy-Images-Aks.ps1` script to deploy latest version of the FoundationaLLM images during initial setup.  However, you may want to update the images to a specific version.  This can be done by updating the values file and redeploying the images.

## PREREQUISITES

- Azure CLI set to the correct tenant/subscription context
- AKS credentials located in the `.kube/config` directory

    To obtain AKS credentials, use the following Azure CLI command:

    ```pwsh
    az aks get-credentials --name "[PREFIX]-aks" --resource-group "[DEPLOYMENT RESOURCE GROUP]"
    ```
- Helm CLI

## DESCRIPTION

This script deploys multiple images on an AKS (Azure Kubernetes Service) cluster using Helm charts.
It takes various parameters such as the release name, AKS name, resource group, tag, charts to deploy,
values file, namespace, TLS/SSL environment, TLS host, TLS secret name, and autoscale option.

## PARAMETER name

The release name for the deployment. Default is "foundationallm".

## PARAMETER aksName

The name of the AKS cluster.

## PARAMETER resourceGroup

The resource group of the AKS cluster.

## PARAMETER tag

The tag of the images to deploy. Default is "latest".

## PARAMETER charts

The charts to deploy. Use "*" to deploy all charts. Default is "*".

## PARAMETER valuesFile

The path to the values file for Helm charts. Default is "gvalues.yaml".

## PARAMETER namespace

The namespace to deploy the charts. If empty, it uses the namespace specified in .kube/config.

## PARAMETER tlsEnv

The TLS/SSL environment to enable. Valid values are "prod", "staging", "none", and "custom". Default is "prod".

## PARAMETER tlsHost

The hostname of the AKS cluster. Required if tlsEnv is set to "custom".

## PARAMETER tlsSecretName

The name of the TLS secret. Required if tlsEnv is set to "custom".

## PARAMETER autoscale

Specifies whether to enable autoscaling for the core-job chart. Default is $false.

## EXAMPLE

```powershell
Deploy-Images-Aks.ps1 `
    -aksName "myAKS" `
    -resourceGroup "myResourceGroup" `
    -tag "v1.0" `
    -charts "core-api,core-job" `
    -valuesFile "myvalues.yaml" `
    -namespace "myNamespace" `
    -tlsEnv "prod" `
    -tlsHost "myaks.example.com" `
    -tlsSecretName "myTLSSecret" `
    -autoscale $true
```

>**Note:** The script will NOT generate a `gvalues.yaml` file for you. If you do not have a `gvalues.yaml` file locally, use the Helm CLI to obtain it.

```pwsh
# Get all releases (e.g., foundationallm-web)
helm list
# Write gvalues.yaml for release
helm get values "foundationallm-web" --all > gvalues.yaml
```
