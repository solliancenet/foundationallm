#!/usr/bin/pwsh

<#
.SYNOPSIS
    Uploads system prompts to Azure storage containers.

.DESCRIPTION
    This script uploads system prompts to Azure storage containers.

.PARAMETER resourceGroup
    Specifies the name of the resource group where the storage account is located. This parameter is mandatory.

.PARAMETER location
    Specifies the location of the storage account. This parameter is mandatory.

.EXAMPLE
    UploadSystemPrompts.ps1 -resourceGroup "myResourceGroup" -location "westus"
#>

Param(
    [parameter(Mandatory = $true)][string]$resourceGroup,
    [parameter(Mandatory = $true)][string]$location
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
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

$storageAccount = Invoke-AndRequireSuccess "Getting storage account name" {
    az storage account list `
        --resource-group $resourceGroup `
        --query '[0].name' `
        --output tsv
}

$target = "https://$storageAccount.blob.core.windows.net/resource-provider/"

& ../tools/azcopy/azcopy copy '../../common/data/resource-provider/*' $target `
    --exclude-pattern .git* --recursive=True --overwrite=True
