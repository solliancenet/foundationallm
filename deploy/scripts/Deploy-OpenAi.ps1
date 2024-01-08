#!/usr/bin/pwsh

Param(
    # Mandatory
    [parameter(Mandatory = $true)][string]$completionsDeployment,
    [parameter(Mandatory = $true)][string]$completionsDeployment4,
    [parameter(Mandatory = $true)][string]$embeddingsDeployment,
    [parameter(Mandatory = $true)][string]$location,
    [parameter(Mandatory = $true)][string]$name,
    [parameter(Mandatory = $true)][string]$resourceGroup,

    # Optional
    [parameter(Mandatory = $false)][string]$completionsModelName = 'gpt-35-turbo',
    [parameter(Mandatory = $false)][string]$completionsModelVersion = '0301',
    [parameter(Mandatory = $false)][string]$completionsModelName4 = 'gpt-4',
    [parameter(Mandatory = $false)][string]$completionsModelVersion4 = '1106-Preview'
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

Push-Location $($MyInvocation.InvocationName | Split-Path)

if (-Not (az cognitiveservices account list -g $resourceGroup --query '[].name' -o json | ConvertFrom-Json) -Contains $name) {
    Write-Host "The Azure OpenAI account $($name) was not found, creating it..." -ForegroundColor Yellow

    az cognitiveservices account create `
        --custom-domain $name `
        --kind OpenAI `
        --location $location `
        --name $name `
        --resource-group $resourceGroup `
        --sku S0 `
        --yes 
        
    if (-Not (az cognitiveservices account list -g $resourceGroup --query '[].name' -o json | ConvertFrom-Json) -Contains $name) {
        Write-Error "The Azure OpenAI account $($name) was not found, and could not be created." -ForegroundColor Red
        Pop-Location
        exit 1
    }
}

$deployments = (az cognitiveservices account deployment list -g $resourceGroup -n $name --query '[].name' -o json | ConvertFrom-Json)
Write-Host "Existing deployments: $($deployments)" -ForegroundColor Blue

if (-Not ($deployments -Contains $completionsDeployment)) {
    Write-Host "The Azure OpenAI deployment $($completionsDeployment) under account $($name) was not found, creating it..." -ForegroundColor Yellow

    az cognitiveservices account deployment create `
        --deployment-name $completionsDeployment `
        --model-format OpenAI `
        --model-name $completionsModelName `
        --model-version $completionsModelVersion `
        --name $name `
        --resource-group $resourceGroup `
        --sku Standard `
        --sku-capacity 30 

    $deployments = (az cognitiveservices account deployment list -g $resourceGroup -n $name --query '[].name' -o json | ConvertFrom-Json)
    if (-Not ($deployments -Contains $completionsDeployment)) {
        Write-Error "The Azure OpenAI deployment $($completionsDeployment) under account $($name) was not found, and could not be created." -ForegroundColor Red
        Pop-Location
        exit 1
    }
}

if (-Not ($deployments -Contains $completionsDeployment4)) {
    Write-Host "The Azure OpenAI deployment $($completionsDeployment4) under account $($name) was not found, creating it..." -ForegroundColor Yellow

    az cognitiveservices account deployment create `
        --deployment-name $completionsDeployment4 `
        --model-format OpenAI `
        --model-name $completionsModelName4 `
        --model-version $completionsModelVersion4 `
        --name $name `
        --resource-group $resourceGroup `
        --sku Standard `
        --sku-capacity 30 

    $deployments = (az cognitiveservices account deployment list -g $resourceGroup -n $name --query '[].name' -o json | ConvertFrom-Json)
    if (-Not ($deployments -Contains $completionsDeployment4)) {
        Write-Error "The Azure OpenAI deployment $($completionsDeployment4) under account $($name) was not found, and could not be created." -ForegroundColor Red
        Pop-Location
        exit 1
    }
}

if (-Not ($deployments -Contains $embeddingsDeployment)) {
    Write-Host "The Azure OpenAI deployment $($embeddingsDeployment) under account $($name) was not found, creating it..." -ForegroundColor Yellow

    az cognitiveservices account deployment create `
        --deployment-name $embeddingsDeployment `
        --model-format OpenAI `
        --model-name 'text-embedding-ada-002' `
        --model-version '2' `
        --name $name `
        --resource-group $resourceGroup `
        --sku Standard `
        --sku-capacity 120 

    $deployments = (az cognitiveservices account deployment list -g $resourceGroup -n $name --query '[].name' -o json | ConvertFrom-Json)
    if (-Not ($deployments -Contains $embeddingsDeployment)) {
        Write-Error "The Azure OpenAI deployment $($embeddingsDeployment) under account $($name) was not found, and could not be created." -ForegroundColor Red
        Pop-Location
        exit 1
    }
}

Pop-Location