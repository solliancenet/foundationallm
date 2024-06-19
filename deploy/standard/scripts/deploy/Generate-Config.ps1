#! /usr/bin/pwsh

Param (
    [parameter(Mandatory = $true)][object]$adminGroupObjectId,
    [parameter(Mandatory = $true)][object]$entraClientIds,
    [parameter(Mandatory = $true)][object]$entraScopes,
    [parameter(Mandatory = $true)][object]$ingress,
    [parameter(Mandatory = $true)][object]$instanceId,
    [parameter(Mandatory = $true)][object]$resourceGroups,
    [parameter(Mandatory = $true)][object]$serviceNamespaceName,
    [parameter(Mandatory = $true)][string]$resourceSuffix,
    [parameter(Mandatory = $true)][string]$subscriptionId
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Get-CIDRHost {
    param(
        [string]$baseCidr,
        [int]$hostNum
    )

    $parts = $baseCidr -split '/'
    $baseIp = $parts[0]
    $subnetMask = [int]$parts[1]

    $totalHosts = [math]::Pow(2, (32 - $subnetMask)) - 2 # Subtract 2 for network and broadcast addresses

    if ($hostNum -lt 0 -or $hostNum -gt $totalHosts) {
        throw "Host number is out of range for the given CIDR block"
    }

    $baseIpBinary = [System.Net.IPAddress]::Parse($baseIp).GetAddressBytes()
    [Array]::Reverse($baseIpBinary)
    $baseIpLong = [System.BitConverter]::ToUInt32($baseIpBinary, 0)

    $offset = $hostNum
    $specificIpLong = $baseIpLong + $offset
    $specificIpBinary = [System.BitConverter]::GetBytes($specificIpLong)
    [Array]::Reverse($specificIpBinary)

    $specificIp = [System.Net.IPAddress]::new($specificIpBinary)
    return $specificIp.ToString()
}

function Invoke-AndRequireSuccess {
    <#
    .SYNOPSIS
    Invokes a script block and requires it to execute successfully.

    .DESCRIPTION
    The Invoke-AndRequireSuccess function is used to invoke a script block and ensure that it executes successfully. It takes a message and a script block as parameters. The function will display the message in blue color, execute the script block, and check the exit code. If the exit code is non-zero, an exception will be thrown.

    .PARAMETER Message
    The message to be displayed before executing the script block.

    .PARAMETER ScriptBlock
    The script block to be executed.

    .EXAMPLE
    Invoke-AndRequireSuccess -Message "Running script" -ScriptBlock {
        # Your script code here
    }

    This example demonstrates how to use the Invoke-AndRequireSuccess function to run a script block and require it to execute successfully.

    #>
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

function PopulateTemplate {
    param(
        [parameter(Mandatory = $true, Position = 0)][object]$tokens,
        [parameter(Mandatory = $true, Position = 1)][string]$template,
        [parameter(Mandatory = $true, Position = 2)][string]$output
    )

    $templatePath = $(
        ../../common/scripts/Join-Path-Recursively `
            -pathParts $template.Split(",")
    ) | Resolve-Path
    Write-Host "Template: $templatePath" -ForegroundColor Blue

    $outputFilePath = $(
        ../../common/scripts/Join-Path-Recursively `
            -pathParts $output.Split(",")
    )
    # This works when output file doesn't exist
    $outputFilePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($outputFilePath)
    Write-Host "Output: $outputFilePath" -ForegroundColor Blue

    ../../common/scripts/Token-Replace.ps1 `
        -inputFile $templatePath `
        -outputFile $outputFilePath `
        -tokens $tokens
}

$svcResourceSuffix = "${resourceSuffix}-svc"
$tokens = @{}

$authServices = @{
    authorizationapi = @{
        miName         = "mi-authorization-api-$svcResourceSuffix"
        miConfigName   = "authorizationApiMiClientId"
        ingressEnabled = $false
    }
}

$services = @{
    orchestrationapi         = @{
        miName         = "mi-orchestration-api-$svcResourceSuffix"
        miConfigName   = "orchestrationApiMiClientId"
        ingressEnabled = $false
    }
    agenthubapi              = @{
        miName         = "mi-agent-hub-api-$svcResourceSuffix"
        miConfigName   = "agentHubApiMiClientId"
        ingressEnabled = $false
    }
    chatui                   = @{
        miName         = "mi-chat-ui-$svcResourceSuffix"
        miConfigName   = "chatUiMiClientId"
        ingressEnabled = $true
        hostname       = "www.internal.foundationallm.ai"
    }
    coreapi                  = @{
        miName         = "mi-core-api-$svcResourceSuffix"
        miConfigName   = "coreApiMiClientId"
        ingressEnabled = $true
        hostname       = "api.internal.foundationallm.ai"
    }
    corejob                  = @{
        miName         = "mi-core-job-$svcResourceSuffix"
        miConfigName   = "coreJobMiClientId"
        ingressEnabled = $false
    }
    datasourcehubapi         = @{
        miName         = "mi-data-source-hub-api-$svcResourceSuffix"
        miConfigName   = "dataSourceHubApiMiClientId"
        ingressEnabled = $false
    }
    gatekeeperapi            = @{
        miName         = "mi-gatekeeper-api-$svcResourceSuffix"
        miConfigName   = "gatekeeperApiMiClientId"
        ingressEnabled = $false
    }
    gatekeeperintegrationapi = @{
        miName         = "mi-gatekeeper-integration-api-$svcResourceSuffix"
        miConfigName   = "gatekeeperIntegrationApiMiClientId"
        ingressEnabled = $false
    }
    gatewayapi               = @{
        miName         = "mi-gateway-api-$svcResourceSuffix"
        miConfigName   = "gatewayApiMiClientId"
        ingressEnabled = $false
    }
    langchainapi             = @{
        miName         = "mi-langchain-api-$svcResourceSuffix"
        miConfigName   = "langChainApiMiClientId"
        ingressEnabled = $false
    }
    managementapi            = @{
        miName         = "mi-management-api-$svcResourceSuffix"
        miConfigName   = "managementApiMiClientId"
        ingressEnabled = $true
        hostname       = "management-api.internal.foundationallm.ai"
    }
    managementui             = @{
        miName         = "mi-management-ui-$svcResourceSuffix"
        miConfigName   = "managementUiMiClientId"
        ingressEnabled = $true
        hostname       = "management.internal.foundationallm.ai"
    }
    prompthubapi             = @{
        miName         = "mi-prompt-hub-api-$svcResourceSuffix"
        miConfigName   = "promptHubApiMiClientId"
        ingressEnabled = $false
    }
    semantickernelapi        = @{
        miName         = "mi-semantic-kernel-api-$svcResourceSuffix"
        miConfigName   = "semanticKernelApiMiClientId"
        ingressEnabled = $false
    }
    vectorizationapi         = @{
        miName         = "mi-vectorization-api-$svcResourceSuffix"
        miConfigName   = "vectorizationApiMiClientId"
        ingressEnabled = $false
    }
    vectorizationjob         = @{
        miName         = "mi-vectorization-job-$svcResourceSuffix"
        miConfigName   = "vectorizationJobMiClientId"
        ingressEnabled = $false
    }
}

$tokens.deployTime = $((Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fffffffZ'))
$tokens.contributorRoleAssignmentGuid = $(New-Guid).Guid
$tokens.userAccessAdminRoleAssignmentGuid = $(New-Guid).Guid
$tokens.managementApiRoleAssignmentGuid = $(New-Guid).Guid
$tokens.coreApiRoleAssignmentGuid = $(New-Guid).Guid
$tokens.vectorizationApiRoleAssignmentGuid = $(New-Guid).Guid
$tokens.orchestrationApiRoleAssignmentGuid = $(New-Guid).Guid
$tokens.subscriptionId = $subscriptionId
$tokens.storageResourceGroup = $resourceGroups.storage
$tokens.opsResourceGroup = $resourceGroups.ops

$tokens.adminGroupObjectId = $adminGroupObjectId

$tokens.chatEntraClientId = $entraClientIds.chat
$tokens.chatEntraScopes = $entraScopes.chat
$tokens.coreApiHostname = $ingress.apiIngress.coreapi.host
$tokens.coreEntraClientId = $entraClientIds.core
$tokens.coreEntraScopes = $entraScopes.core
$tokens.instanceId = $instanceId
$tokens.managementApiEntraClientId = $entraClientIds.managementapi
$tokens.managementApiEntraScopes = $entraScopes.managementapi
$tokens.managementApiHostname = $ingress.apiIngress.managementapi.host
$tokens.managementEntraClientId = $entraClientIds.managementui
$tokens.managementEntraScopes = $entraScopes.managementui
$tokens.opsResourceGroup = $resourceGroups.ops
$tokens.storageResourceGroup = $resourceGroups.storage
$tokens.subscriptionId = $subscriptionId
$tokens.authorizationApiScope = $entraScopes.authorization

$tenantId = Invoke-AndRequireSuccess "Get Tenant ID" {
    az account show --query homeTenantId --output tsv
}
$tokens.tenantId = $tenantId

$vectorizationConfig = Invoke-AndRequireSuccess "Get Vectorization Config" {
    $content = Get-Content -Raw -Path "../config/vectorization.json" | `
        ConvertFrom-Json | `
        ConvertTo-Json -Compress -Depth 50
    return $content.Replace('"', '\"')
}
$tokens.vectorizationConfig = $vectorizationConfig

$openAiEndpointUri = Invoke-AndRequireSuccess "Get OpenAI endpoint" {
    az cognitiveservices account list `
        --output tsv `
        --query "[?contains(kind,'OpenAI')] | [0].properties.endpoint" `
        --resource-group $($resourceGroups.oai) 
}
$tokens.openAiEndpointUri = $openAiEndpointUri

$openAiAccountName = Invoke-AndRequireSuccess "Get OpenAI Account name" {
    az cognitiveservices account list `
        --output tsv `
        --query "[?contains(kind,'OpenAI')] | [0].id" `
        --resource-group $($resourceGroups.oai) 
}
$tokens.azureOpenAiAccountName = $openAiAccountName

$appConfig = Invoke-AndRequireSuccess "Get AppConfig Instance" {
    az appconfig list `
        --resource-group $($resourceGroups.ops) `
        --query "[0].{name:name,endpoint:endpoint}" `
        --output json | `
        ConvertFrom-Json
}
$tokens.appConfigName = $appConfig.name
$tokens.appConfigEndpoint = $appConfig.endpoint

$appConfigCredential = Invoke-AndRequireSuccess "Get AppConfig Credential" {
    az appconfig credential list `
        --name $appConfig.name `
        --resource-group $($resourceGroups.ops) `
        --query "[?name=='Primary Read Only'].{connectionString: connectionString}" `
        --output json | `
        ConvertFrom-Json
}
$tokens.appConfigConnectionString = $appConfigCredential.connectionString

$appConfigRWCredential = Invoke-AndRequireSuccess "Get AppConfig Credential" {
    az appconfig credential list `
        --name $appConfig.name `
        --resource-group $($resourceGroups.ops) `
        --query "[?name=='Primary'].{connectionString: connectionString}" `
        --output json | `
        ConvertFrom-Json
}
$tokens.appConfigRWConnectionString = $appConfigRWCredential.connectionString

$cogSearchName = Invoke-AndRequireSuccess "Get Cognitive Search endpoint" {
    az search service list `
        --resource-group $resourceGroups.vec `
        --query "[0].name" `
        --output tsv
}
$tokens.cognitiveSearchEndpointUri = "https://$($cogSearchName).search.windows.net"

$backendAks = Invoke-AndRequireSuccess "Get Backend AKS" {
    az aks list `
        --resource-group $($resourceGroups.app) `
        --query "[?contains(name, 'backend')].addonProfiles.azureKeyvaultSecretsProvider.identity.clientId | [0]" `
        --output tsv
}
$tokens.aksBackendCsiIdentityClientId = $backendAks

$frontendAks = Invoke-AndRequireSuccess "Get Frontend AKS" {
    az aks list `
        --resource-group $($resourceGroups.app) `
        --query "[?contains(name, 'frontend')].addonProfiles.azureKeyvaultSecretsProvider.identity.clientId | [0]" `
        --output tsv
}
$tokens.aksFrontendCsiIdentityClientId = $frontendAks

$contentSafetyUri = Invoke-AndRequireSuccess "Get Content Safety endpoint" {
    az cognitiveservices account list `
        --resource-group $($resourceGroups.oai) `
        --query "[?kind=='ContentSafety'].properties.endpoint" `
        --output tsv
}
$tokens.contentSafetyEndpointUri = $contentSafetyUri

$docDbEndpoint = Invoke-AndRequireSuccess "Get CosmosDB endpoint" {
    az cosmosdb list `
        --resource-group $($resourceGroups.storage) `
        --query "[?kind=='GlobalDocumentDB'].documentEndpoint" `
        --output tsv
}
$tokens.cosmosEndpoint = $docDbEndpoint

$eventGridNamespace = Invoke-AndRequireSuccess "Get Event Grid Namespace" {
    az eventgrid namespace list `
        --resource-group $($resourceGroups.app) `
        --query "[0].{hostname:topicsConfiguration.hostname, id:id}" `
        --output json | `
        ConvertFrom-Json
}
$tokens.eventGridNamespaceEndpoint = "https://$($eventGridNamespace.hostname)/"
$tokens.eventGridNamespaceId = $eventGridNamespace.id

$keyVault = Invoke-AndRequireSuccess "Get Key Vault URI" {
    az keyvault list `
        --resource-group $($resourceGroups.ops) `
        --query "[0].{uri:properties.vaultUri,name:name}" `
        --output json | `
        ConvertFrom-Json
}
$tokens.keyVaultName = $keyVault.name
$tokens.keyvaultUri = $keyvault.uri

$authKeyvault = Invoke-AndRequireSuccess "Get Auth Key Vault URI" {
    az keyvault list `
        --resource-group $($resourceGroups.auth) `
        --query "[0].{uri:properties.vaultUri,name:name}" `
        --output json | `
        ConvertFrom-Json
}
$tokens.authKvUri = $authKeyvault.uri

$vnetName = Invoke-AndRequireSuccess "Get VNet Name" {
    az network vnet list `
        --output tsv `
        --query "[0].name" `
        --resource-group $resourceGroups.net `
        --output tsv
}

$subnetBackend = Invoke-AndRequireSuccess "Get Backend Subnet CIDR" {
    az network vnet subnet show `
        --name "FLLMBackend" `
        --query addressPrefix `
        --resource-group $resourceGroups.net `
        --vnet-name $vnetName `
        --output tsv
}
$tokens.privateIpIngressBackend = Get-CIDRHost -baseCidr $subnetBackend -hostNum 250

$subnetFrontend = Invoke-AndRequireSuccess "Get Frontend Subnet CIDR" {
    az network vnet subnet show `
        --name "FLLMFrontend" `
        --query addressPrefix `
        --resource-group $resourceGroups.net `
        --vnet-name $vnetName `
        --output tsv
}
$tokens.privateIpIngressFrontend = Get-CIDRHost -baseCidr $subnetFrontend -hostNum 250

$storageAccountAdlsName = Invoke-AndRequireSuccess "Get ADLS Storage Account" {
    az storage account list `
        --resource-group $($resourceGroups.storage) `
        --query "[?kind=='StorageV2'].name" `
        --output tsv
}
$tokens.storageAccountAdlsName = $storageAccountAdlsName

foreach ($service in $services.GetEnumerator()) {
    $miClientId = Invoke-AndRequireSuccess "Get $($service.Key) managed identity" {
        az identity show `
            --resource-group $($resourceGroups.app) `
            --name $($service.Value.miName) `
            --query "clientId" `
            --output tsv
    }

    $miObjectId = Invoke-AndRequireSuccess "Get $($service.Key) managed identity object ID" {
        az identity show `
            --resource-group $($resourceGroups.app) `
            --name $($service.Value.miName) `
            --query "principalId" `
            --output tsv
    }

    $service.Value.miClientId = $miClientId
    $service.Value.miObjectId = $miObjectId
}

foreach ($service in $authServices.GetEnumerator()) {
    $miClientId = Invoke-AndRequireSuccess "Get $($service.Key) managed identity" {
        az identity show `
            --resource-group $($resourceGroups.auth) `
            --name $($service.Value.miName) `
            --query "clientId" `
            --output tsv
    }

    $service.Value.miClientId = $miClientId
}

$tokens.orchestrationApiMiClientId = $services["orchestrationapi"].miClientId
$tokens.orchestrationApiMiObjectId = $services["orchestrationapi"].miObjectId
$tokens.agentHubApiMiClientId = $services["agenthubapi"].miClientId
$tokens.authorizationApiMiClientId = $authServices["authorizationapi"].miClientId
$tokens.chatUiMiClientId = $services["chatui"].miClientId
$tokens.coreApiMiClientId = $services["coreapi"].miClientId
$tokens.coreApiMiObjectId = $services["coreapi"].miObjectId
$tokens.coreJobMiClientId = $services["corejob"].miClientId
$tokens.dataSourceHubApiMiClientId = $services["datasourcehubapi"].miClientId
$tokens.gatekeeperApiMiClientId = $services["gatekeeperapi"].miClientId
$tokens.gatekeeperIntegrationApiMiClientId = $services["gatekeeperintegrationapi"].miClientId
$tokens.langChainApiMiClientId = $services["langchainapi"].miClientId
$tokens.managementApiMiClientId = $services["managementapi"].miClientId
$tokens.managementApiMiObjectId = $services["managementapi"].miObjectId
$tokens.managementUiMiClientId = $services["managementui"].miClientId
$tokens.promptHubApiMiClientId = $services["prompthubapi"].miClientId
$tokens.semanticKernelApiMiClientId = $services["semantickernelapi"].miClientId
$tokens.vectorizationApiMiClientId = $services["vectorizationapi"].miClientId
$tokens.vectorizationApiMiObjectId = $services["vectorizationapi"].miObjectId
$tokens.vectorizationJobMiClientId = $services["vectorizationjob"].miClientId
$tokens.gatewayApiMiClientId = $services["gatewayapi"].miClientId

$eventGridProfiles = @{}
$eventGridProfileNames = @(
    "orchestration-api-event-profile"
    "core-api-event-profile"
    "vectorization-api-event-profile"
    "vectorization-worker-event-profile"
    "management-api-event-profile"
)
foreach ($profileName in $eventGridProfileNames) {
    Write-Host "Populating $profileName..." -ForegroundColor Blue

    PopulateTemplate $tokens `
        "..,config,$($profileName).template.json" `
        "..,config,$($profileName).json"

    $eventGridProfiles[$profileName] = $(
        Get-Content -Raw -Path "../config/$($profileName).json" | `
            ConvertFrom-Json | `
            ConvertTo-Json -Compress -Depth 50
    ).Replace('"', '\"')
}

$tokens.orchestrationApiEventGridProfile = $eventGridProfiles["orchestration-api-event-profile"]
$tokens.coreApiEventGridProfile = $eventGridProfiles["core-api-event-profile"]
$tokens.vectorizationApiEventGridProfile = $eventGridProfiles["vectorization-api-event-profile"]
$tokens.vectorizationWorkerEventGridProfile = $eventGridProfiles["vectorization-worker-event-profile"]
$tokens.managementApiEventGridProfile = $eventGridProfiles["management-api-event-profile"]
$tokens.authKeyvaultUri = $authKeyvault.uri

PopulateTemplate $tokens "..,config,appconfig.template.json" "..,config,appconfig.json"
PopulateTemplate $tokens "..,config,kubernetes,spc.foundationallm-certificates.backend.template.yml" "..,config,kubernetes,spc.foundationallm-certificates.backend.yml"
PopulateTemplate $tokens "..,config,kubernetes,spc.foundationallm-certificates.frontend.template.yml" "..,config,kubernetes,spc.foundationallm-certificates.frontend.yml"
PopulateTemplate $tokens "..,config,helm,ingress-nginx.values.backend.template.yml" "..,config,helm,ingress-nginx.values.backend.yml"
PopulateTemplate $tokens "..,config,helm,ingress-nginx.values.frontend.template.yml" "..,config,helm,ingress-nginx.values.frontend.yml"
PopulateTemplate $tokens "..,config,helm,internal-service.template.yml" "..,config,helm,microservice-values.yml"
PopulateTemplate $tokens "..,data,resource-provider,FoundationaLLM.Agent,FoundationaLLM.template.json" "..,..,common,data,resource-provider,FoundationaLLM.Agent,FoundationaLLM.json"
PopulateTemplate $tokens "..,data,resource-provider,FoundationaLLM.Prompt,FoundationaLLM.template.json" "..,..,common,data,resource-provider,FoundationaLLM.Prompt,FoundationaLLM.json"

$($ingress.apiIngress).PSObject.Properties | ForEach-Object {
    $tokens.serviceBaseUrl = $_.Value.path
    $tokens.serviceHostname = $_.Value.host
    $tokens.serviceName = $_.Value.serviceName
    $tokens.serviceNamespaceName = $serviceNamespaceName
    $tokens.servicePath = $_.Value.path + "(.*)"
    $tokens.servicePathType = $_.Value.pathType
    $tokens.serviceSecretName = $_.Value.sslCert
    PopulateTemplate $tokens "..,config,helm,exposed-service.template.yml" "..,config,helm,$($_.Name)-values.yml"
    PopulateTemplate $tokens "..,config,helm,service-ingress.template.yml" "..,config,helm,$($_.Name)-ingress.yml"
}

$($ingress.frontendIngress).PSObject.Properties | ForEach-Object {
    $tokens.serviceBaseUrl = $_.Value.path
    $tokens.serviceHostname = $_.Value.host
    $tokens.serviceName = $_.Value.serviceName
    $tokens.serviceNamespaceName = $serviceNamespaceName
    $tokens.servicePath = $_.Value.path + "(.*)"
    $tokens.servicePathType = $_.Value.pathType
    $tokens.serviceSecretName = $_.Value.sslCert
    PopulateTemplate $tokens "..,config,helm,frontend-service.template.yml" "..,config,helm,$($_.Name)-values.yml"
    PopulateTemplate $tokens "..,config,helm,service-ingress.template.yml" "..,config,helm,$($_.Name)-ingress.yml"
}

PopulateTemplate $tokens "..,data,role-assignments,DefaultRoleAssignments.template.json" "..,data,role-assignments,$($instanceId).json"

exit 0
