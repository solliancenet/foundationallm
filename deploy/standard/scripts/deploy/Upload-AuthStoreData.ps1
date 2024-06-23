#! /usr/bin/pwsh
<#
.SYNOPSIS
    Uploads authorization RBAC settings to Azure storage container.

.DESCRIPTION
    This script uploads authorization RBAC settings to Azure storage container.

.PARAMETER resourceGroup
    Specifies the name of the resource group where the storage account is located. This parameter is mandatory.

.PARAMETER instanceId
    Specifies the instance of the FLLM install which is found in the Deployment-Manifest.json file on this machine. This parameter is mandatory.

.EXAMPLE
    UploadAuthStoreData.ps1 -resourceGroup "myResourceGroup" -instanceId "myInstanceId" 
#>

Param (
    [parameter(Mandatory = $true)][string]$resourceGroup,
    [parameter(Mandatory = $true)][string]$instanceId
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Invoke-AndRequireSuccess {
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    Write-Host "${message}..." -ForegroundColor Blue
    $result = & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        throw "Failed ${message} (code: ${LASTEXITCODE})"
    }

    return $result
}

$storageAccountAdls = Invoke-AndRequireSuccess "Get ADLS Auth Storage Account" {
    az storage account list `
        --resource-group $resourceGroup `
        --query "[?kind=='StorageV2'].name | [0]" `
        --output tsv
}

if (-not (Test-Path "../data/role-assignments/$($instanceId)`.json")) {
    throw "Default role assignments json not found at ../data/role-assignments/$($instanceId)`.json"
}

$target = "https://$storageAccountAdls.blob.core.windows.net/role-assignments/"

& ../tools/azcopy/azcopy cp "../data/role-assignments/$($instanceId)`.json" "$target"
