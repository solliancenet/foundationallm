#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $true)][string]$resourceGroup,
    [parameter(Mandatory = $true)][string]$location,
    [parameter(Mandatory = $true)][string]$subscription,
    [parameter(Mandatory = $false)][string]$armTemplate = $null,
    [parameter(Mandatory = $false)][string]$openAiName = $null,
    [parameter(Mandatory = $false)][string]$openAiRg = $null,
    [parameter(Mandatory = $false)][string]$openAiCompletionsDeployment = $null,
    [parameter(Mandatory = $false)][string]$openAiCompletionsDeployment4 = $null,
    [parameter(Mandatory = $false)][string]$openAiEmbeddingsDeployment = $null,
    [parameter(Mandatory = $false)][bool]$stepDeployArm = $true,
    [parameter(Mandatory = $false)][bool]$stepDeployOpenAi = $true,
    [parameter(Mandatory = $false)][bool]$deployAks = $false,
    [parameter(Mandatory = $false)][bool]$stepDeployCertManager = $true,
    [parameter(Mandatory = $false)][bool]$stepDeployTls = $true,
    [parameter(Mandatory = $false)][bool]$stepDeployImages = $true,
    [parameter(Mandatory = $false)][bool]$stepUploadSystemPrompts = $true,
    [parameter(Mandatory = $false)][bool]$stepLoginAzure = $true,
    [parameter(Mandatory = $false)][string]$resourcePrefix = $null
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$gValuesFile = "configFile.yaml"

Push-Location $($MyInvocation.InvocationName | Split-Path)

# Update the extension to make sure you have the latest version installed
az extension add --name aks-preview
az extension update --name aks-preview

az extension add --name  application-insights
az extension update --name  application-insights

az extension add --name storage-preview
az extension update --name storage-preview

az extension add --name containerapp
az extension update --name containerapp

if ($stepLoginAzure) {
    # Write-Host "Login in your account" -ForegroundColor Yellow
    az login
}

# Write-Host "Choosing your subscription" -ForegroundColor Yellow
az account set --subscription $subscription

if (-Not (az group list --query "[?name=='$resourceGroup'].name" -o json | ConvertFrom-Json) -Contains $resourceGroup) {
    Write-Host "The resource group $resourceGroup was not found, creating it..." -ForegroundColor Yellow
    $rg = $(az group create -g $resourceGroup -l $location --subscription $subscription)
    if (-Not (az group list --query "[?name=='$resourceGroup'].name" -o json | ConvertFrom-Json) -Contains $resourceGroup) {
        Write-Error("The resource group $resourceGroup was not found, and could not be created.")
        exit 1
    }
}

if (-not $resourcePrefix) {
    $crypt = New-Object -TypeName System.Security.Cryptography.SHA256Managed
    $utf8 = New-Object -TypeName System.Text.UTF8Encoding
    $hash = [System.BitConverter]::ToString($crypt.ComputeHash($utf8.GetBytes($resourceGroup)))
    $hash = $hash.replace('-', '').toLower()
    $resourcePrefix = "fllm$($hash.Substring(0, 5))"
}

if ($stepDeployOpenAi) {
    if (-not $openAiRg) {
        $openAiRg = $resourceGroup
    }

    if (-not $openAiName) {
        $openAiName = "$($resourcePrefix)-openai"
    }

    if (-not $openAiCompletionsDeployment) {
        $openAiCompletionsDeployment = "completions"
    }

    if (-not $openAiCompletionsDeployment4) {
        $openAiCompletionsDeployment4 = "completions4"
    }

    if (-not $openAiEmbeddingsDeployment) {
        $openAiEmbeddingsDeployment = "embeddings"
    }

    try {
        & ./Deploy-OpenAi.ps1 `
            -name $openAiName `
            -resourceGroup $openAiRg `
            -location $location `
            -completionsDeployment $openAiCompletionsDeployment `
            -completionsDeployment4 $openAiCompletionsDeployment4 `
            -completionsModelVersion '0613' `
            -embeddingsDeployment $openAiEmbeddingsDeployment
    }
    finally {
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error deploying OpenAI" -ForegroundColor Red
            Pop-Location
            exit $LASTEXITCODE
        }
    }
}

## Getting OpenAI info
if ($openAiName) {
    $openAi = $(az cognitiveservices account show -n $openAiName -g $openAiRg -o json | ConvertFrom-Json)
    $openAiEndpoint = $openAi.properties.endpoint
}
else {
    $openAi = $(az cognitiveservices account list -g $resourceGroup -o json | ConvertFrom-Json)
    $openAiRg = $resourceGroup
}

$openAiKey = $(az cognitiveservices account keys list -g $openAiRg -n $openAi.name -o json --query key1 | ConvertFrom-Json)

if ($stepDeployArm) {

    if ([string]::IsNullOrEmpty($armTemplate)) {
        if ($deployAks) {
            $armTemplate = "azureAksDeploy.json"
        }
        else {
            $armTemplate = "azureAcaDeploy.json"
        }
    }
    # Deploy ARM
    & ./Deploy-Arm-Azure.ps1 `
        -resourcePrefix $resourcePrefix `
        -resourceGroup $resourceGroup `
        -location $location `
        -template $armTemplate `
        -deployAks $deployAks `
        -openAiEndpoint $openAiEndpoint `
        -openAiKey $openAiKey `
        -openAiCompletionsDeployment $openAiCompletionsDeployment `
        -openAiEmbeddingsDeployment $openAiEmbeddingsDeployment
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error deploying ARM" -ForegroundColor Red
        Pop-Location
        exit $LASTEXITCODE
    }
}

if ($stepUploadSystemPrompts) {
    # Upload System Prompts
    & ./UploadSystemPrompts.ps1 -resourceGroup $resourceGroup -location $location
}

if ($deployAks) {
    # Connecting kubectl to AKS
    Write-Host "Retrieving Aks Name" -ForegroundColor Yellow
    $aksName = $(az aks list -g $resourceGroup -o json | ConvertFrom-Json).name
    Write-Host "The name of your AKS: $aksName" -ForegroundColor Yellow

    # Write-Host "Retrieving credentials" -ForegroundColor Yellow
    az aks get-credentials -n $aksName -g $resourceGroup --overwrite-existing

    # Generate Config
    New-Item -ItemType Directory -Force -Path $(./Join-Path-Recursively.ps1 -pathParts .., __values)
    $gValuesLocation = $(./Join-Path-Recursively.ps1 -pathParts .., __values, $gValuesFile)
    & ./Generate-Config.ps1 `
        -resourceGroup $resourceGroup `
        -openAiName $openAiName `
        -openAiRg $openAiRg `
        -outputFile $gValuesLocation `
        -deployAks $deployAks
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error generating config" -ForegroundColor Red
        exit $LASTEXITCODE
    }

    if ($stepDeployCertManager) {
        # Deploy Cert Manager
        & ./DeployCertManager.ps1
    }

    if ($stepDeployTls) {
        # Deploy TLS
        & ./DeployTlsSupport.ps1 -sslSupport prod -resourceGroup $resourceGroup -aksName $aksName
    }

    if ($stepDeployImages) {
        # Deploy images in AKS
        $gValuesLocation = $(./Join-Path-Recursively.ps1 -pathParts .., __values, $gValuesFile)
        $chartsToDeploy = "*"

        & ./Deploy-Images-Aks.ps1 -aksName $aksName -resourceGroup $resourceGroup -charts $chartsToDeploy -valuesFile $gValuesLocation
    }

    $webappHostname = $(az aks show -n $aksName -g $resourceGroup -o json --query addonProfiles.httpApplicationRouting.config.HTTPApplicationRoutingZoneName | ConvertFrom-Json)
    $coreApiUri = "https://$webappHostname/core"
}
else {
    $webappHostname = $(az deployment group show -g $resourceGroup -n foundationallm-azuredeploy -o json --query properties.outputs.webFqdn.value | ConvertFrom-Json)
    $coreApiHostname = $(az deployment group show -g $resourceGroup -n foundationallm-azuredeploy -o json --query properties.outputs.coreApiFqdn.value | ConvertFrom-Json)
    $coreApiUri = "https://$coreApiHostname"
}

Write-Host "===========================================================" -ForegroundColor Yellow
Write-Host "The frontend is hosted at https://$webappHostname" -ForegroundColor Yellow
Write-Host "The Core API is hosted at $coreApiUri" -ForegroundColor Yellow
Write-Host "===========================================================" -ForegroundColor Yellow

Pop-Location
