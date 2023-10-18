#!/usr/bin/pwsh

Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$location
)

Push-Location $($MyInvocation.InvocationName | Split-Path)
Push-Location $(Join-Path .. "data")

$storageAccount = $(az storage account list -g $resourceGroup -o json | ConvertFrom-Json).name
az storage container create --account-name $storageAccount --name "agents" --only-show-errors
az storage azcopy blob upload -c agents --account-name $storageAccount -s "./agents/*" --recursive --only-show-errors

az storage container create --account-name $storageAccount --name "data-sources" --only-show-errors
az storage azcopy blob upload -c data-sources --account-name $storageAccount -s "./data-sources/*" --recursive --only-show-errors

az storage container create --account-name $storageAccount --name "foundationallm-source" --only-show-errors
az storage azcopy blob upload -c foundationallm-source --account-name $storageAccount -s "./foundationallm-source/*" --recursive --only-show-errors

az storage container create --account-name $storageAccount --name "system-prompt" --only-show-errors
az storage azcopy blob upload -c system-prompt --account-name $storageAccount -s "./system-prompt/*" --recursive --only-show-errors

Pop-Location
Pop-Location
