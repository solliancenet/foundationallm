#! /usr/bin/pwsh
<#
.SYNOPSIS
    Generates a set of FLLM EntraID App Registrations and their respective client apps in the Entra ID tenant.
    See the following link for more information: https://docs.foundationallm.ai/deployment/authentication-authorization/index.html
    **These app names are mandatory, you can't change the names or the scopes.**
    
.DESCRIPTION
    The script will create the following apps:
    - FoundationaLLM-Authorization-API
    - FoundationaLLM-Core-API
    - FoundationaLLM-Core-Portal
    - FoundationaLLM-Management-API
    - FoundationaLLM-Management-Portal
    - FoundationaLLM-Reader
    
    The script will also assign the required permissions to the client apps and the required API permissions to the API apps. 

    Users can be added as Owners of the app registrations by creating an `admins.json` file in the `deploy/common/config` folder
    with the following JSON array content

    ```json
    [
        "user1@example.com",
        "user2@example.com"
    ]
    ```
    > Note: Only members of the resident tenant will be able to be imported in this manner.  Accounts outside of the tenant will
      be ignored.

.REQUIREMENTS
    - The user must be a Global Administrator in the Entra ID tenant or have RBAC rights to create App Registrations and Service Principals.
    - The Azure CLI must be installed and authenticated to the Entra ID tenant.
    - Scaffolding JSON files must be present in the same directory as the script.
        - foundationallm-authorization-api.template.json  
        - foundationallm-core-api.template.json
        - foundationallm-core-portal.template.json
        - foundationallm-management-api.template.json
        - foundationallm-management-portal.template.json
        - foundationallm-reader.template.json

.PARAMETER appPermissionsId
The GUID of the permission to assign to the client app.

.PARAMETER appUrl
The URL of the client app.

.PARAMETER appUrlLocal
The local URL of the client app.

.PARAMETER createClientApp
If set to $true, the script will create a client app. If set to $false, the script will not create a client app.

.PARAMETER fllmApi
The name of the API app.

.PARAMETER fllmApiConfigPath
The path to the API app configuration file.

.PARAMETER fllmApiUri
The URI of the API app.

.PARAMETER fllmClient
The name of the client app.

.PARAMETER fllmClientConfigPath
The path to the client app configuration file.


.EXAMPLE
The following example creates the FoundationaLLM API and client apps.

# Create FoundationaLLM Core App Registrations
$params = @{
    appPermissionsId     = "6da07102-bb6a-421d-a71e-dfdb6031d3d8"
    appUrl               = ""
    appUrlLocal          = "http://localhost:3000/signin-oidc"
    fllmApi              = $coreAppName
    fllmApiConfigPath    = "foundationallm-core-api.json"
    fllmApiUri           = "api://FoundationaLLM-Core"
    fllmClient           = $coreClientAppName
    fllmClientConfigPath = "foundationallm-core-portal.json"
}
$($fllmAppRegs).Core = New-FllmEntraIdApps @params
    
#>

Param(
    [parameter(Mandatory = $false)][string]$authAppName="FoundationaLLM-Authorization-API",
    [parameter(Mandatory = $false)][string]$coreAppName="FoundationaLLM-Core-API",
    [parameter(Mandatory = $false)][string]$coreClientAppName="FoundationaLLM-Core-Portal",
    [parameter(Mandatory = $false)][string]$mgmtAppName="FoundationaLLM-Management-API",
    [parameter(Mandatory = $false)][string]$mgmtClientAppName="FoundationaLLM-Management-Portal",
    [parameter(Mandatory = $false)][string]$readerAppName="FoundationaLLM-Reader"
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function New-FllmEntraIdApps {
    param (
        [Parameter(Mandatory = $false)][array]$ownerObjectIds=@(),
        [Parameter(Mandatory = $true)][string]$appPermissionsId,
        [Parameter(Mandatory = $false)][string]$appUrl,
        [Parameter(Mandatory = $false)][string]$appUrlLocal,
        [Parameter(Mandatory = $false)][bool]$createClientApp = $true,
        [Parameter(Mandatory = $true)][string]$fllmApi,
        [Parameter(Mandatory = $true)][string]$fllmApiConfigPath,
        [Parameter(Mandatory = $true)][string]$fllmApiUri,
        [Parameter(Mandatory = $false)][string]$fllmClient,
        [Parameter(Mandatory = $false)][string]$fllmClientConfigPath,
        [Parameter(Mandatory = $false)][string]$isApp = $true
    )

    $fllmAppRegMetaData = @{}
    try {
        # Create the FLLM API App Registration
        $($fllmAppRegMetaData).Api = @{ 
            Name = $fllmApi
            Uri = $fllmApiUri
        }
        Write-Host -ForegroundColor Yellow "Creating EntraID Application Registration named $($fllmAppRegMetaData.Api.Name)"
        $($fllmAppRegMetaData.Api).AppId = $(az ad app create --display-name $($fllmAppRegMetaData.Api.Name) --query appId --output tsv)
        $($fllmAppRegMetaData.Api).ObjectId = $(az ad app show --id $($fllmAppRegMetaData.Api.AppId) --query id --output tsv)

        foreach ($objectId in $ownerObjectIds) {
            az ad app owner add --id $($fllmAppRegMetaData.Api.AppId) --owner-object-id $objectId
        }        
        
        az ad sp create --id $($fllmAppRegMetaData.Api.AppId) 

        if ($isApp -eq $true) {
            # Create the FLLM ClientApp Registration
            if ($createClientApp) {
                $($fllmAppRegMetaData).Client = @{ Name = $fllmClient }
                Write-Host -ForegroundColor Yellow "Creating EntraID Application Registration named $($fllmAppRegMetaData.Client.Name)"    
                $($fllmAppRegMetaData.Client).AppId = $(az ad app create --display-name $($fllmAppRegMetaData.Client.Name) --query appId --output tsv)
                $($fllmAppRegMetaData.Client).ObjectId = $(az ad app show --id $($fllmAppRegMetaData.Client.AppId) --query id --output tsv)
                az ad sp create --id $($fllmAppRegMetaData.Client.AppId)

                foreach ($objectId in $ownerObjectIds) {
                    az ad app owner add --id $($fllmAppRegMetaData.Client.AppId) --owner-object-id $objectId
                }        
            }

            # Update the API App Registration
            Write-Host -ForegroundColor Yellow "Laying down scaffolding for the API App Registration $($fllmAppRegMetaData.Api.Name)"
            az rest --method PATCH --url "https://graph.microsoft.com/v1.0/applications/$($fllmAppRegMetaData.Api.ObjectId)" --header "Content-Type=application/json" --body "@$fllmApiConfigPath"
            Write-host -ForegroundColor Blue "Sleeping for 10 seconds to allow the API App Registration to be created before updating it..."
            Start-Sleep -Seconds 10
            ## Updates the API App Registration
            Write-Host -ForegroundColor Yellow "Preparing updates for the API App Registration $($fllmAppRegMetaData.Api.Name)"
            $appConfig = Get-content $fllmApiConfigPath | ConvertFrom-Json -Depth 20
            $preAuthorizedApp = @(
                @{
                    "appId" = "04b07795-8ddb-461a-bbee-02f9e1bf7b46";
                    "delegatedPermissionIds" = @("$($appPermissionsId)")
                }
            )

            if ($createClientApp) {
                $preAuthorizedApp += @{
                    "appId" = $($fllmAppRegMetaData.Client.AppId); 
                    "delegatedPermissionIds" = @("$($appPermissionsId)") 
                }
            }

            $appConfig.api.preAuthorizedApplications = $preAuthorizedApp
            $appConfig.identifierUris = @($($fllmAppRegMetaData.Api.Uri))
            $appConfigUpdate = $appConfig | ConvertTo-Json -Depth 20
            Write-Host -ForegroundColor Yellow "Final Update to API App Registration $($fllmAppRegMetaData.Api.Name)"
            Set-Content -Path "$($fllmAppRegMetaData.Api.Name)`.json" $appConfigUpdate
            az rest --method PATCH --url "https://graph.microsoft.com/v1.0/applications/$($fllmAppRegMetaData.Api.ObjectId)" --header "Content-Type=application/json" --body "@$($fllmAppRegMetaData.Api.Name)`.json"

            # Update the Client App Registration
            if ($createClientApp) {     
                Write-Host -ForegroundColor Yellow "Lay down scaffolding for the Client App Registration $($fllmAppRegMetaData.Client.Name)"
                az rest --method PATCH --url "https://graph.microsoft.com/v1.0/applications/$($fllmAppRegMetaData.Client.ObjectId)" --header "Content-Type=application/json" --body "@$fllmClientConfigPath"
                Start-Sleep -Seconds 10
                Write-host -ForegroundColor Blue "Sleeping for 10 seconds to allow the API App Registration to be created before updating it..."
                ## Updates the Client App Registration
                Write-Host -ForegroundColor Yellow "Preparing updates for the Client App Registration $($fllmAppRegMetaData.Client.Name)"
                $($fllmAppRegMetaData.Client).Uri = @("api://$($fllmAppRegMetaData.Client.Name)")
                $apiPermissions = @(@{"resourceAppId" = $($fllmAppRegMetaData.Api.AppId); "resourceAccess" = @(@{"id" = "$($appPermissionsId)"; "type" = "Scope" }) }, @{"resourceAppId" = "00000003-0000-0000-c000-000000000000"; "resourceAccess" = @(@{"id" = "e1fe6dd8-ba31-4d61-89e7-88639da4683d"; "type" = "Scope" }) })
                $appConfig = Get-content $fllmClientConfigPath | ConvertFrom-Json -Depth 20
                $appConfig.identifierUris = @($($fllmAppRegMetaData.Client.Uri))
                $appConfig.requiredResourceAccess = $apiPermissions
                $appConfigUpdate = $appConfig | ConvertTo-Json -Depth 20
                Write-Host -ForegroundColor Yellow "Applying Final Update to Client App Registration $($fllmAppRegMetaData.Client.Name)..."
                Set-Content -Path "$($fllmAppRegMetaData.Client.Name)`.json" $appConfigUpdate
                az rest --method PATCH --url "https://graph.microsoft.com/v1.0/applications/$($fllmAppRegMetaData.Client.ObjectId)" --header "Content-Type=application/json" --body "@$($fllmAppRegMetaData.Client.Name)`.json"
            }
        }
    }
    catch {
        Write-Host "Error occurred: $_"
    }

    return $fllmAppRegMetaData
}

$fllmAppRegs = @{}
$ownerObjectIds = @()

if (Test-Path ../config/admins.json -PathType Leaf) {
    $ownerAccounts = Get-Content ../config/admins.json -Raw | ConvertFrom-Json
    foreach ($account in $ownerAccounts) {
        $objectId = $(az ad user show --id $account --query "id" -o tsv)
        if ($objectId) {
            $ownerObjectIds += $objectId
        }
    }
}

# Create FoundationaLLM Core App Registrations
$params = @{
    ownerObjectIds       = $ownerObjectIds
    appPermissionsId     = "6da07102-bb6a-421d-a71e-dfdb6031d3d8"
    appUrl               = ""
    appUrlLocal          = "http://localhost:3000/signin-oidc"
    fllmApi              = $coreAppName
    fllmApiConfigPath    = "foundationallm-core-api.template.json"
    fllmApiUri           = "api://FoundationaLLM-Core"
    fllmClient           = $coreClientAppName
    fllmClientConfigPath = "foundationallm-core-portal.template.json"
}
$($fllmAppRegs).Core = New-FllmEntraIdApps @params

# Create FoundationaLLM Management App Registrations
$params = @{
    ownerObjectIds       = $ownerObjectIds
    appPermissionsId     = "c57f4633-0e58-455a-8ede-5de815fe6c9c"
    appUrl               = ""
    appUrlLocal          = "http://localhost:3001/signin-oidc"
    fllmApi              = $mgmtAppName
    fllmApiConfigPath    = "foundationallm-management-api.template.json"
    fllmApiUri           = "api://FoundationaLLM-Management"
    fllmClient           = $mgmtClientAppName
    fllmClientConfigPath = "foundationallm-management-portal.template.json"
}
$($fllmAppRegs).Management = New-FllmEntraIdApps @params

# Create FoundationaLLM Authorization App Registration
$params = @{
    ownerObjectIds    = $ownerObjectIds
    appPermissionsId  = "9e313dd4-51e4-4989-84d0-c713e38e467d"
    createClientApp   = $false
    fllmApi           = $authAppName
    fllmApiConfigPath = "foundationallm-authorization-api.template.json"
    fllmApiUri        = "api://FoundationaLLM-Authorization"
}
$($fllmAppRegs).Authorization = New-FllmEntraIdApps @params

$params = @{
    ownerObjectIds    = $ownerObjectIds
    appPermissionsId  = "9e313dd4-51e4-4989-84d0-c713e38e467d"
    createClientApp   = $false
    fllmApi           = $readerAppName
    fllmApiConfigPath = "foundationallm-reader.template.json"
    fllmApiUri        = "api://FoundationaLLM-Reader"
    isApp             = $false
}
$($fllmAppRegs).Reader = New-FllmEntraIdApps @params

Write-Host $($fllmAppRegs | ConvertTo-Json)
