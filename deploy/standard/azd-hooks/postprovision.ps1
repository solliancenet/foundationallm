#!/usr/bin/env pwsh

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Load utility functions
Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    . ./utility/Load-Utility-Functions.ps1
}
finally {
    Pop-Location
}

# Navigate to the script directory so that we can use relative paths.
Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    # Create VNET peerings
    Invoke-AndRequireSuccess "Provision VNET Peering to Hub" {
        $peerings = @(az network vnet peering list `
                        --resource-group $env:FLLM_NET_RG `
                        --subscription $env:AZURE_SUBSCRIPTION_ID `
                        --vnet-name $env:FOUNDATIONALLM_VNET_NAME `
                        --query "[].name" `
                        --output json | ConvertFrom-Json)

        if ($peerings.Contains("$($env:FOUNDATIONALLM_VNET_NAME)-to-$($env:FOUNDATIONALLM_HUB_VNET_NAME)")) {
            Write-Host "Peering for $($env:FOUNDATIONALLM_VNET_NAME)-to-$($env:FOUNDATIONALLM_HUB_VNET_NAME) exists..."
        } else {
            Write-Host "Creating peering for $($env:FOUNDATIONALLM_VNET_NAME)-to-$($env:FOUNDATIONALLM_HUB_VNET_NAME)"
            az network vnet peering create `
                --name "$($env:FOUNDATIONALLM_VNET_NAME)-to-$($env:FOUNDATIONALLM_HUB_VNET_NAME)" `
                --remote-vnet $env:FOUNDATIONALLM_HUB_VNET_ID `
                --resource-group $env:FLLM_NET_RG `
                --vnet-name $env:FOUNDATIONALLM_VNET_NAME `
                --subscription $env:AZURE_SUBSCRIPTION_ID `
                --allow-forwarded-traffic 1 `
                --allow-gateway-transit 0 `
                --allow-vnet-access 1 `
                --use-remote-gateways 0
        }

        $peerings = @(az network vnet peering list `
                        --resource-group $env:FOUNDATIONALLM_HUB_RESOURCE_GROUP `
                        --subscription $env:FOUNDATIONALLM_HUB_SUBSCRIPTION_ID `
                        --vnet-name $env:FOUNDATIONALLM_HUB_VNET_NAME `
                        --query "[].name" `
                        --output json | ConvertFrom-Json)

        if ($peerings.Contains("$($env:FOUNDATIONALLM_HUB_VNET_NAME)-to-$($env:FOUNDATIONALLM_VNET_NAME)")) {
            Write-Host "Peering for $($env:FOUNDATIONALLM_HUB_VNET_NAME)-to-$($env:FOUNDATIONALLM_VNET_NAME) exists..."
        } else {
            Write-Host "Creating peering for $($env:FOUNDATIONALLM_HUB_VNET_NAME)-to-$($env:FOUNDATIONALLM_VNET_NAME)"
            az network vnet peering create `
                --name "$($env:FOUNDATIONALLM_HUB_VNET_NAME)-to-$($env:FOUNDATIONALLM_VNET_NAME)" `
                --remote-vnet $env:FOUNDATIONALLM_VNET_ID `
                --resource-group $env:FOUNDATIONALLM_HUB_RESOURCE_GROUP `
                --vnet-name $env:FOUNDATIONALLM_HUB_VNET_NAME `
                --subscription $env:FOUNDATIONALLM_HUB_SUBSCRIPTION_ID `
                --allow-forwarded-traffic 1 `
                --allow-gateway-transit 0 `
                --allow-vnet-access 1 `
                --use-remote-gateways 0
        }
    }


    # Convert the manifest resource groups to a hashtable for easier access
    $resourceGroup = @{
        app     = $env:FLLM_APP_RG
        auth    = $env:FLLM_AUTH_RG
        data    = $env:FLLM_DATA_RG
        jbx     = $env:FLLM_JBX_RG
        net     = $env:FLLM_NET_RG
        oai     = $env:FLLM_OAI_RG
        ops     = $env:FLLM_OPS_RG
        storage = $env:FLLM_STORAGE_RG
        vec     = $env:FLLM_VEC_RG
    }

    Invoke-AndRequireSuccess "Generate Host File" {
        ./utility/Generate-Hosts.ps1 `
            -resourceGroup $resourceGroup `
            -subscription $env:AZURE_SUBSCRIPTION_ID
    }
}
finally {
    Pop-Location
    Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
}