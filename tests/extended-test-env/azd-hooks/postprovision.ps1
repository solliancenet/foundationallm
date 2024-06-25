#!/usr/bin/env pswh

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Invoke-CLICommand {
    <#
    .SYNOPSIS
    Invoke a CLI Command and allow all output to print to the terminal.  Does not check for return values or pass the output to the caller.
    #>
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    Write-Host "${message}..." -ForegroundColor Blue
    & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        throw "Failed ${message} (code: ${LASTEXITCODE})"
    }
}

$deployments = @{
    "phi" = @{
        deploymentName= $env:PHI_DEPLOYMENT_NAME
        onlineEndpoint= $env:PHI_ENDPOINT_NAME
    }
    "mistral" = @{
        deploymentName= $env:MISTRAL_DEPLOYMENT_NAME
        onlineEndpoint= $env:MISTRAL_ENDPOINT_NAME

    }
}

Invoke-CLICommand "Install AZ CLI ML Extension" {
    az extension add --name ml --upgrade --yes
}

Invoke-CLICommand "Setting Azure Subscription" {
    az account set -s $env:AZURE_SUBSCRIPTION_ID
}


foreach($deployment in $deployments.GetEnumerator()) {
    $deploymentName = $deployment.Value.deploymentName
    $onlineEndpoint = $deployment.Value.onlineEndpoint

    Invoke-CLICommand "Updating Online Endpoint" {
        az ml online-endpoint update `
            --name $onlineEndpoint `
            --resource-group $env:RESOURCE_GROUP_NAME `
            --traffic "${deploymentName}=100" `
            --workspace-name $env:PROJECT_WORKSPACE_NAME
    }
}