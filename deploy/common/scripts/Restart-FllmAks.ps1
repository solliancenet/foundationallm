#! /usr/bin/pwsh
<#
.SYNOPSIS
    Restarts all containers in specified namespaces within multiple AKS clusters in a single resource group.

.DESCRIPTION
    This script logs into Azure, sets the specified subscription, retrieves the credentials for each AKS cluster within the same resource group,
    and restarts all containers in the specified namespaces by using `kubectl rollout restart` for deployments
    and updating the daemonsets. The script requires Azure CLI and kubectl to be installed and configured.

.PARAMETER subscriptionId
    The subscription ID for the Azure account.

.PARAMETER resourceGroupName
    The name of the resource group containing the AKS clusters.

.PARAMETER aksClusterNames
    The array of AKS cluster names within the resource group.

.PARAMETER namespaces
    The array of namespaces within the AKS clusters where the containers will be restarted. Defaults to "gateway-system" and "fllm".

.EXAMPLE
    ./Restart-FllmAks.ps1 -resourceGroupName "your-resource-group" -aksClusterNames @("aks1", "aks2")
    This example shows how to run the script for multiple AKS clusters with default namespaces "gateway-system" and "fllm".

.NOTES
	This script can be used to restart all containers in the specified namespaces across multiple AKS clusters in the FLLM environment.
	Make sure to have the Azure CLI and kubectl installed and authenticated. 
#>

param (
    [Parameter(Mandatory = $true)][string[]]$aksClusterNames,
    [Parameter(Mandatory = $true)][string]$resourceGroupName,
    [Parameter(Mandatory = $false)][string[]]$namespaces = @("gateway-system", "fllm")
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

foreach ($aksClusterName in $aksClusterNames) {
    Write-Host -ForegroundColor Blue "Processing AKS cluster: $aksClusterName in resource group: $resourceGroupName"

    # Get credentials for the AKS cluster
    Write-Host -ForegroundColor Yellow "Getting AKS Credentials for AKS cluster: $aksClusterName..."
	az aks get-credentials --resource-group $resourceGroupName --name $aksClusterName

    foreach ($namespace in $namespaces) {
        Write-Host -ForegroundColor Yellow "Processing namespace: $namespace in AKS cluster: $aksClusterName"

        # Restart all deployments in the namespace
        kubectl rollout restart deployment -n $namespace

        # Restart all daemonsets to trigger a rolling update
        $daemonsets = kubectl get daemonsets -n $namespace -o jsonpath='{.items[*].metadata.name}'
        foreach ($daemonset in $daemonsets) {
            kubectl rollout restart daemonset $daemonset -n $namespace
        }
    }
}

Write-Host -ForegroundColor Green "All containers in the specified namespaces across all specified AKS clusters have been restarted."