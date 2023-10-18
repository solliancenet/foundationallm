#! /usr/bin/pwsh

Param (
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$false)][string]$openAiName,
    [parameter(Mandatory=$false)][string]$openAiRg,
    [parameter(Mandatory=$false)][string]$openAiDeployment,
    [parameter(Mandatory=$false)][string[]]$outputFile=$null,
    [parameter(Mandatory=$false)][string[]]$gvaluesTemplate="..,gvalues.template.yml",
    [parameter(Mandatory=$false)][string[]]$migrationSettingsTemplate="..,migrationsettings.template.json",
    [parameter(Mandatory=$false)][string]$ingressClass="addon-http-application-routing",
    [parameter(Mandatory=$false)][string]$domain,
    [parameter(Mandatory=$true)][string]$deployAks
)

function EnsureAndReturnFirstItem($arr, $restype) {
    if (-not $arr -or $arr.Length -ne 1) {
        Write-Host "Fatal: No $restype found (or found more than one)" -ForegroundColor Red
        exit 1
    }

    return $arr[0]
}

# Check the rg
$rg=$(az group show -n $resourceGroup -o json | ConvertFrom-Json)

if (-not $rg) {
    Write-Host "Fatal: Resource group not found" -ForegroundColor Red
    exit 1
}

### Getting Resources
$tokens=@{}

## Getting storage info
# $storage=$(az storage account list -g $resourceGroup --query "[].{name: name, blob: primaryEndpoints.blob}" -o json | ConvertFrom-Json)
# $storage=EnsureAndReturnFirstItem $storage "Storage Account"
# Write-Host "Storage Account: $($storage.name)" -ForegroundColor Yellow

## Getting API URL domain
if ($deployAks)
{
    if ([String]::IsNullOrEmpty($domain)) {
        $domain = $(az aks show -n $aksName -g $resourceGroup -o json --query addonProfiles.httpApplicationRouting.config.HTTPApplicationRoutingZoneName | ConvertFrom-Json)
        if (-not $domain) {
            $domain = $(az aks show -n $aksName -g $resourceGroup -o json --query addonProfiles.httpapplicationrouting.config.HTTPApplicationRoutingZoneName | ConvertFrom-Json)
        }
    }
}
else
{
    $domain=$(az deployment group show -g $resourceGroup -n foundationallm-azuredeploy -o json --query properties.outputs.apiFqdn.value | ConvertFrom-Json)
}

$apiUrl = "https://$domain"
Write-Host "API URL: $apiUrl" -ForegroundColor Yellow

$appConfig=$(az appconfig list -g $resourceGroup -o json | ConvertFrom-Json).name
$appConfigEndpoint=$(az appconfig show -g $resourceGroup -n $appConfig --query 'endpoint' -o json | ConvertFrom-Json)
$appConfigConnectionString=$(az appconfig credential list -n $appConfig -g $resourceGroup --query "[?name=='Primary Read Only'].{connectionString: connectionString}" -o json | ConvertFrom-Json).connectionString

## Getting CosmosDb info
$docdb=$(az cosmosdb list -g $resourceGroup --query "[?kind=='GlobalDocumentDB'].{name: name, kind:kind, documentEndpoint:documentEndpoint}" -o json | ConvertFrom-Json)
$docdb=EnsureAndReturnFirstItem $docdb "CosmosDB (Document Db)"
$docdbKey=$(az cosmosdb keys list -g $resourceGroup -n $docdb.name -o json --query primaryMasterKey | ConvertFrom-Json)
Write-Host "Document Db Account: $($docdb.name)" -ForegroundColor Yellow

$resourcePrefix=$(az deployment group show -n foundationallm-azuredeploy -g $resourceGroup --query "properties.outputs.resourcePrefix.value" -o json | ConvertFrom-Json)
$agentFactoryApiMiClientId=$(az identity show -g $resourceGroup -n $resourcePrefix-agent-factory-mi -o json | ConvertFrom-Json).clientId
$agentHubApiMiClientId=$(az identity show -g $resourceGroup -n $resourcePrefix-agent-hub-mi -o json | ConvertFrom-Json).clientId
$chatUiMiClientId=$(az identity show -g $resourceGroup -n $resourcePrefix-chat-ui-mi -o json | ConvertFrom-Json).clientId
$coreApiMiClientId=$(az identity show -g $resourceGroup -n $resourcePrefix-core-mi -o json | ConvertFrom-Json).clientId
$dataSourceHubApiMiClientId=$(az identity show -g $resourceGroup -n $resourcePrefix-data-source-hub-mi -o json | ConvertFrom-Json).clientId
$gatekeeperApiMiClientId=$(az identity show -g $resourceGroup -n $resourcePrefix-gatekeeper-mi -o json | ConvertFrom-Json).clientId
$langChainApiMiClientId=$(az identity show -g $resourceGroup -n $resourcePrefix-langchain-mi -o json | ConvertFrom-Json).clientId
$promptHubApiMiClientId=$(az identity show -g $resourceGroup -n $resourcePrefix-prompt-hub-mi -o json | ConvertFrom-Json).clientId
$semanticKernelApiMiClientId=$(az identity show -g $resourceGroup -n $resourcePrefix-semantic-kernel-mi -o json | ConvertFrom-Json).clientId
$tenantId=$(az account show --query homeTenantId --output tsv)

## Showing Values that will be used

Write-Host "===========================================================" -ForegroundColor Yellow
Write-Host "gvalues file will be generated with values:"

$tokens.apiUrl=$apiUrl
$tokens.cosmosConnectionString="AccountEndpoint=$($docdb.documentEndpoint);AccountKey=$docdbKey"
$tokens.cosmosEndpoint=$docdb.documentEndpoint
$tokens.cosmosKey=$docdbKey
$tokens.agentFactoryApiMiClientId=$agentFactoryApiMiClientId
$tokens.agentHubApiMiClientId=$agentHubApiMiClientId
$tokens.chatUiMiClientId=$chatUiMiClientId
$tokens.coreApiMiClientId=$coreApiMiClientId
$tokens.dataSourceHubApiMiClientId=$dataSourceHubApiMiClientId
$tokens.gatekeeperApiMiClientId=$gatekeeperApiMiClientId
$tokens.langChainApiMiClientId=$langChainApiMiClientId
$tokens.promptHubApiMiClientId=$promptHubApiMiClientId
$tokens.semanticKernelApiMiClientId=$semanticKernelApiMiClientId
$tokens.tenantId=$tenantId
$tokens.appConfigEndpoint=$appConfigEndpoint
$tokens.appConfigConnectionString=$appConfigConnectionString

# Standard fixed tokens
$tokens.ingressclass=$ingressClass
$tokens.ingressrewritepath="(/|$)(.*)"
$tokens.ingressrewritetarget="`$2"

if($ingressClass -eq "nginx") {
    $tokens.ingressrewritepath="(/|$)(.*)" 
    $tokens.ingressrewritetarget="`$2"
}

Write-Host ($tokens | ConvertTo-Json) -ForegroundColor Yellow
Write-Host "===========================================================" -ForegroundColor Yellow

Push-Location $($MyInvocation.InvocationName | Split-Path)
$gvaluesTemplatePath=$(./Join-Path-Recursively -pathParts $gvaluesTemplate.Split(","))
$outputFilePath=$(./Join-Path-Recursively -pathParts $outputFile.Split(","))
& ./Token-Replace.ps1 -inputFile $gvaluesTemplatePath -outputFile $outputFilePath -tokens $tokens
Pop-Location

Push-Location $($MyInvocation.InvocationName | Split-Path)
$migrationSettingsTemplatePath=$(./Join-Path-Recursively -pathParts $migrationSettingsTemplate.Split(","))
$outputFilePath=$(./Join-Path-Recursively -pathParts ..,migrationsettings.json)
& ./Token-Replace.ps1 -inputFile $migrationSettingsTemplatePath -outputFile $outputFilePath -tokens $tokens
Pop-Location