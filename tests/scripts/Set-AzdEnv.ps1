#!/usr/bin/pwsh
<#
.SYNOPSIS
    Generates a azd environment values to be used with the Starter deployment. This script is best used after running the 
    ./deploy/common/scripts/entraid/Create-FllmEntraApps.ps1 script to create your FLLM EntraID apps.

.DESCRIPTION
    This script generates a set of azd environment values required for the Starter deployment.
    It retrieves the values of the Application IDs of the EntraID Apps required for the FLLM application and assigns them
    using the azd env command. This script must be run after running azd init in the ./deployment/starter folder.

    For more information on seting up the FLLM EntraID apps: : https://docs.foundationallm.ai/deployment/authentication/index.html
    
.PARAMETERS
Mandatory parameters:
    - tenantID: The Azure EntraID tenant ID.

Optional parameters:
    The names of the FLLM apps. If you are using the Create-FllmEntraApps.ps1 script, then the names of the apps are hardcoded and will not need to be set. 
    If you are not using the Create-FllmEntraApps.ps1 script, then you can use the following parameters to set the names of the apps:
    - fllmApiName: The name of the FLLM Core API.
    - fllmClientName: The name of the FLLM Client UI.
    - fllmMgmtApiName: The name of the FLLM Management API.
    - fllmMgmtClientName: The name of the FLLM Management Client UI.
    - fllmAuthApiName: The name of the FLLM Authorization API.
    
.EXAMPLE
If you have run the Create-FllmEntraApps.ps1 script, then you can run the Set-AzdEnv.ps1 script with the tenant ID as the only parameter:
    ./Set-AzdEnv.ps1 -tenant "12345678-1234-1234-1234-1234567890ab"    

If you have not run the Create-FllmEntraApps.ps1 script, then you can run the Set-AzdEnv.ps1 script with the tenant ID and the names of the apps.
You will need to update the app names with those you created manually in the EntraID portal.
    ./Set-AzdEnv.ps1 -tenant "12345678-1234-1234-1234-1234567890ab" `
                     -fllmApiName "FoundationaLLM" `
                     -fllmClientName "FoundationaLLM-Client" `
                     -fllmMgmtApiName "FoundationaLLM-Management" `
                     -fllmMgmtClientName "FoundationaLLM-ManagementClient" `
                     -fllmAuthApiName "FoundationaLLM-Authorization"

Make sure that your API Scope Names are updated inside of the script as well. These can be found in the EntraID portal under the 
Manage -> "Expose and API" section for the API applications you created manually. The Scope name format is api://<api-name>or<api-guid>/<scope-name>.
#>

Param(
    [parameter(Mandatory = $true)][string]$tenantId, # Azure EntraID Tenant
    [parameter(Mandatory = $false)][string]$fllmApiName = "FoundationaLLM-E2E", # FLLM Core API
    [parameter(Mandatory = $false)][string]$fllmClientName = "FoundationaLLM-E2E-Client", # FLLM UI
    [parameter(Mandatory = $false)][string]$fllmMgmtApiName = "FoundationaLLM-Management-E2E", # FLLM Management API
    [parameter(Mandatory = $false)][string]$fllmMgmtClientName = "FoundationaLLM-Management-E2E-Client", # FLLM Management UI
    [parameter(Mandatory = $false)][string]$fllmAuthApiName = "FoundationaLLM-Authorization-E2E", # FLLM Authorization API
    [parameter(Mandatory = $false)][string]$fllmAdminGroupName = "FLLM-E2E-Admins", # FLLM Admin AD Group
    [parameter(Mandatory = $false)][string]$principalType = "User",
    [parameter(Mandatory = $true)][string]$instanceId

)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Set the environment values
$fllmApiAppId = (az ad app list --display-name "$fllmApiName" --query "[].{appId:appId,displayName:displayName}[?displayName=='$fllmApiName'].appId" --output tsv) # FLLM Core API
$fllmClientAppId = (az ad app list --display-name "$fllmClientName" --query "[].{appId:appId,displayName:displayName}[?displayName=='$fllmClientName'].appId" --output tsv) # FLLM UI
$fllmMgmtApiAppId = (az ad app list --display-name "$fllmMgmtApiName" --query "[].{appId:appId,displayName:displayName}[?displayName=='$fllmMgmtApiName'].appId" --output tsv) # FLLM Management API
$fllmMgmtClientAppId = (az ad app list --display-name "$fllmMgmtClientName" --query "[].{appId:appId,displayName:displayName}[?displayName=='$fllmMgmtClientName'].appId" --output tsv) # FLLM Management UI
$fllmAuthApiAppId = (az ad app list --display-name "$fllmAuthApiName" --query "[].{appId:appId,displayName:displayName}[?displayName=='$fllmAuthApiName'].appId" --output tsv) # FLLM Authorization API
$fllmAdminGroupId = (az ad group show --group "$fllmAdminGroupName" --query 'id' --output tsv) # FLLM Admin AD Group

$values = @(
    @{
        Key = "ENTRA_CHAT_UI_CLIENT_ID"
        Value = $fllmClientAppId
    },
    @{
        Key = "ENTRA_CHAT_UI_SCOPES"
        Value = "api://FoundationaLLM-Core/Data.Read"
    },
    @{
        Key = "ENTRA_CHAT_UI_TENANT_ID"
        Value = $tenantId
    },

    @{
        Key = "ENTRA_CORE_API_CLIENT_ID"
        Value = $fllmApiAppId
    },
    @{
        Key = "ENTRA_CORE_API_SCOPES"
        Value = "Data.Read"
    },
    @{
        Key = "ENTRA_CORE_API_TENANT_ID"
        Value = $tenantId
    },

    @{
        Key = "ENTRA_MANAGEMENT_API_CLIENT_ID"
        Value = $fllmMgmtApiAppId
    },
    @{
        Key = "ENTRA_MANAGEMENT_API_SCOPES"
        Value = "Data.Manage"
    },
    @{
        Key = "ENTRA_MANAGEMENT_API_TENANT_ID"
        Value = $tenantId
    },

    @{
        Key = "ENTRA_MANAGEMENT_UI_CLIENT_ID"
        Value = $fllmMgmtClientAppId
    },
    @{
        Key = "ENTRA_MANAGEMENT_UI_SCOPES"
        Value = "api://FoundationaLLM-Management/Data.Manage"
    },
    @{
        Key = "ENTRA_MANAGEMENT_UI_TENANT_ID"
        Value = $tenantId
    },

    @{
        Key = "ENTRA_AUTH_API_CLIENT_ID"
        Value = $fllmAuthApiAppId
    },
    @{
        Key = "ENTRA_AUTH_API_SCOPES"
        Value = "api://FoundationaLLM-Authorization"
    },
    @{
        Key = "ENTRA_AUTH_API_TENANT_ID"
        Value = $tenantId
    },
    @{
        Key = "ENTRA_AUTH_API_INSTANCE"
        Value = "https://login.microsoftonline.com/"
    },

    @{
        Key = "FOUNDATIONALLM_INSTANCE_ID"
        Value = $instanceId
    },

    @{
        Key = "ADMIN_GROUP_OBJECT_ID"
        Value = $fllmAdminGroupId
    },

    @{
        Key = "AZURE_PRINCIPAL_TYPE"
        Value = "ServicePrincipal"
    },

    @{
        Key = "FOUNDATIONALLM_E2E_TEST"
        Value = $true
    }
)

Write-Host "Setting azd environment values for the ${tenantId} EntraID Tenant."

foreach ($value in $values) {
    Write-Host "Setting $value"
    azd env set $($value.Key) "$($value.Value)"

    if ($LASTEXITCODE -ne 0) {
        Write-Error("Failed to set $($value.Key).")
        exit 1
    }
}

Write-Host "Initial azd environment"
$azdEnv=$(azd env get-values)
Write-Host $azdEnv
