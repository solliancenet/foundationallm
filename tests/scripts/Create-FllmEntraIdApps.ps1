#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $false)][string]$authAppName="FoundationaLLM-Authorization-API",
    [parameter(Mandatory = $false)][string]$coreAppName="FoundationaLLM-Core-API",
    [parameter(Mandatory = $false)][string]$coreClientAppName="FoundationaLLM-Core-Portal",
    [parameter(Mandatory = $false)][string]$mgmtAppName="FoundationaLLM-Management-API",
    [parameter(Mandatory = $false)][string]$mgmtClientAppName="FoundationaLLM-Management-Portal"
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

<#
.SYNOPSIS
    Generates a set of FLLM EntraID API apps and their respective client apps in the Azure AD tenant.

.DESCRIPTION
    The script will create the following apps:
    - FoundationaLLM
    - FoundationaLLM-Client
    - FoundationaLLM-Management
    - FoundationaLLM-ManagementClient
    - FoundationaLLM-Authorization
    - 
    The script will also assign the required permissions to the client apps and the required API permissions to the API apps.
    URLs for the client apps are optional and can be set using the appUrl and appUrlLocal parameters.

.PARAMETER appPermissionsId
The GUID of the permission to assign to the client app.

.PARAMETER appUrl
The URL of the client app.

.PARAMETER appUrlLocal
The local URL of the client app.

.PARAMETER createClientApp
Whether to create the client app or not. Default is true. False will only create the API app.

.EXAMPLE
The following example creates the FoundationaLLM API and client apps.

# Create FoundationaLLM Core App Registrations
$params = @{
    fllmApi              = "FoundationaLLM"
    fllmClient           = "FoundationaLLM-Client"
    fllmApiConfigPath    = "foundationalllm.json"
    fllmClientConfigPath = "foundationalllm-client.json"
    appPermissionsId     = "6da07102-bb6a-421d-a71e-dfdb6031d3d8"
    appUrl               = ""
    appUrlLocal          = "http://localhost:3000/signin-oidc"
}
New-FllmEntraIdApps @params
    
#>
function New-FllmEntraIdApps {
    param (
        [Parameter(Mandatory = $true)][string]$appPermissionsId,
        [Parameter(Mandatory = $false)][string]$appUrl,
        [Parameter(Mandatory = $false)][string]$appUrlLocal,
        [Parameter(Mandatory = $false)][bool]$createClientApp = $true,
        [Parameter(Mandatory = $true)][string]$fllmApi,
        [Parameter(Mandatory = $true)][string]$fllmApiConfigPath,
        [Parameter(Mandatory = $true)][string]$fllmApiUri,
        [Parameter(Mandatory = $false)][string]$fllmClient,
        [Parameter(Mandatory = $false)][string]$fllmClientConfigPath       
    )

    $fllmAppRegMetaData = @{}
    try {
        # Create the FLLM APIApp Registration
        $($fllmAppRegMetaData).Api = @{ 
            Name = $fllmApi
            Uri = $fllmApiUri
        }
        Write-Host "Creating EntraID Application Registration named $($fllmAppRegMetaData.Api.Name)"
        $($fllmAppRegMetaData.Api).AppId = $(az ad app create --display-name $($fllmAppRegMetaData.Api.Name) --query appId --output tsv)
        $($fllmAppRegMetaData.Api).ObjectId = $(az ad app show --id $($fllmAppRegMetaData.Api.AppId) --query id --output tsv)
        az ad sp create --id $($fllmAppRegMetaData.Api.AppId) 

        # Create the FLLM ClientApp Registration
        if ($createClientApp) {
            $($fllmAppRegMetaData).Client = @{ Name = $fllmClient }
            Write-Host "Creating EntraID Application Registration named $($fllmAppRegMetaData.Client.Name)"    
            $($fllmAppRegMetaData.Client).AppId = $(az ad app create --display-name $($fllmAppRegMetaData.Client.Name) --query appId --output tsv)
            $($fllmAppRegMetaData.Client).ObjectId = $(az ad app show --id $($fllmAppRegMetaData.Client.AppId) --query id --output tsv)
            az ad sp create --id $($fllmAppRegMetaData.Client.AppId)
        }

        # Update the APIApp Registration
        Write-Host "Lays down scaffolding for the API App Registration $($fllmAppRegMetaData.Api.Name)"
        az rest --method PATCH --url "https://graph.microsoft.com/v1.0/applications/$($fllmAppRegMetaData.Api.ObjectId)" --header "Content-Type=application/json" --body "@$fllmApiConfigPath"
        Write-host "Sleeping for 10 seconds to allow the API App Registration to be created before updating it."
        Start-Sleep -Seconds 10
        ## Updates the API App Registration
        Write-Host "Preparing updates for the API App Registration $($fllmAppRegMetaData.Api.Name)"
        $appConfig = Get-content $fllmApiConfigPath | ConvertFrom-Json -Depth 20
        if ($createClientApp) {
            $preAuthorizedApp = @(
                @{
                    "appId" = $($fllmAppRegMetaData.Client.AppId); 
                    "delegatedPermissionIds" = @("$($appPermissionsId)") 
                },
                @{
                    "appId" = "04b07795-8ddb-461a-bbee-02f9e1bf7b46";
                    "delegatedPermissionIds" = @("$($appPermissionsId)")
                }
            )
            $appConfig.api.preAuthorizedApplications = $preAuthorizedApp
        }
        $appConfig.identifierUris = @($($fllmAppRegMetaData.Api.Uri))
        $appConfigUpdate = $appConfig | ConvertTo-Json -Depth 20
        Write-Host "Final Update to API App Registration $($fllmAppRegMetaData.Api.Name)"
        Set-Content -Path "$($fllmAppRegMetaData.Api.Name)`.json" $appConfigUpdate
        az rest --method PATCH --url "https://graph.microsoft.com/v1.0/applications/$($fllmAppRegMetaData.Api.ObjectId)" --header "Content-Type=application/json" --body "@$($fllmAppRegMetaData.Api.Name)`.json"

        # Update the ClientApp Registration
        if ($createClientApp) {     
            Write-Host "Lay down scaffolding for the ClientApp Registration $($fllmAppRegMetaData.Client.Name)"
            az rest --method PATCH --url "https://graph.microsoft.com/v1.0/applications/$($fllmAppRegMetaData.Client.ObjectId)" --header "Content-Type=application/json" --body "@$fllmClientConfigPath"
            Start-Sleep -Seconds 10
            Write-host "Sleeping for 10 seconds to allow the API App Registration to be created before updating it."
            ## Updates the ClientApp Registration
            Write-Host "Preparing updates for the API App Registration $($fllmAppRegMetaData.Client.Name)"
            $($fllmAppRegMetaData.Client).Uri = @("api://$($fllmAppRegMetaData.Client.Name)")
            $apiPermissions = @(@{"resourceAppId" = $($fllmAppRegMetaData.Client.AppId); "resourceAccess" = @(@{"id" = "$($appPermissionsId)"; "type" = "Scope" }) }, @{"resourceAppId" = "00000003-0000-0000-c000-000000000000"; "resourceAccess" = @(@{"id" = "e1fe6dd8-ba31-4d61-89e7-88639da4683d"; "type" = "Scope" }) })
            $appConfig = Get-content $fllmClientConfigPath | ConvertFrom-Json -Depth 20
            $appConfig.identifierUris = @($($fllmAppRegMetaData.Client.Uri))
            $appConfig.requiredResourceAccess = $apiPermissions
            $appConfigUpdate = $appConfig | ConvertTo-Json -Depth 20
            Write-Host "Final Update to ClientApp Registration $($fllmAppRegMetaData.Client.Name)"
            Set-Content -Path "$($fllmAppRegMetaData.Client.Name)`.json" $appConfigUpdate
            az rest --method PATCH --url "https://graph.microsoft.com/v1.0/applications/$($fllmAppRegMetaData.Client.ObjectId)" --header "Content-Type=application/json" --body "@$($fllmAppRegMetaData.Client.Name)`.json"
        }
    }
    catch {
        Write-Host "Error occurred: $_"
    }

    return $fllmAppRegMetaData
}

$fllmAppRegs = @{}
# Create FoundationaLLM Core App Registrations
$params = @{
    fllmApi              = $coreAppName
    fllmClient           = $coreClientAppName
    fllmApiConfigPath    = "foundationalllm.json"
    fllmApiUri           = "api://FoundationaLLM-Core"
    fllmClientConfigPath = "foundationalllm-client.json"
    appPermissionsId     = "6da07102-bb6a-421d-a71e-dfdb6031d3d8"
    appUrl               = ""
    appUrlLocal          = "http://localhost:3000/signin-oidc"
}
$($fllmAppRegs).Core = New-FllmEntraIdApps @params

# Create FoundationaLLM Management App Registrations
$params = @{
    fllmApi              = $mgmtAppName
    fllmClient           = $mgmtClientAppName
    fllmApiConfigPath    = "foundationalllm-management.json"
    fllmApiUri           = "api://FoundationaLLM-Management"
    fllmClientConfigPath = "foundationalllm-managementclient.json"
    appPermissionsId     = "c57f4633-0e58-455a-8ede-5de815fe6c9c"
    appUrl               = ""
    appUrlLocal          = "http://localhost:3001/signin-oidc"
}
$($fllmAppRegs).Management = New-FllmEntraIdApps @params

# Create FoundationaLLM Authorization App Registration
$params = @{
    fllmApi           = $authAppName
    fllmApiConfigPath = "foundationalllm-authorization.json"
    fllmApiUri        = "api://FoundationaLLM-Authorization"
    appPermissionsId  = "9e313dd4-51e4-4989-84d0-c713e38e467d"
    createClientApp   = $false
}
$($fllmAppRegs).Authorization = New-FllmEntraIdApps  @params

Write-Host $($fllmAppRegs | ConvertTo-Json)
