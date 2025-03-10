#!/usr/bin/pwsh
<#
.SYNOPSIS
    Generates azd environment values to be used with the FLLM deployment.
    This script should be run after running:
    ./deploy/common/scripts/Create-FllmEntraApps.ps1 script to create your FLLM EntraID apps.
    ./deploy/common/scripts/Create-FllmAdminGroup.ps1 script to create your FLLM-Admins Group.

.DESCRIPTION
    This script generates a set of azd environment values required for the deployment.
    It retrieves the values of the Application IDs of the EntraID Apps required for the FLLM application and assigns them
    using the azd env command.

    For more information on setting up the FLLM EntraID apps: https://docs.foundationallm.ai/deployment/authentication/index.html

.PARAMETER tenantID
   The Azure EntraID tenant ID.

.PARAMETER adminGroupName
   The name of the admin group. Default is 'FLLM-Admins'.

.PARAMETER authAppName
   The name of the Authorization API app. Default is 'FoundationaLLM-Authorization-API'.

.PARAMETER coreAppName
   The name of the Core API app. Default is 'FoundationaLLM-Core-API'.

.PARAMETER coreClientAppName
   The name of the Core Portal app. Default is 'FoundationaLLM-Core-Portal'.

.PARAMETER mgmtAppName
   The name of the Management API app. Default is 'FoundationaLLM-Management-API'.

.PARAMETER mgmtClientAppName
   The name of the Management Portal app. Default is 'FoundationaLLM-Management-Portal'.

.EXAMPLE
QuickStart deployment:
from the ./foundationallm/deploy/quickstart directory:
    ../common/scripts/Set-AzdEnvEntra.ps1 -tenantID "12345678-1234-1234-1234-1234567890ab"

Standard deployment:
from the ./foundationallm/deploy/standard directory:
    ../common/scripts/Set-AzdEnvEntra.ps1 -tenantID "12345678-1234-1234-1234-1234567890ab"

.NOTES
    Set the Azure CLI context to the appropriate subscription before running this script.
    You can set the subscription using the 'az account set --subscription' command.

    This script must be run from the ./deploy/quikstart or the .deploy/standard directory after
    created the azd environment using the azd env create command. The values will be populated in the .env file
    located in the hidden .azure directory in the root of the project.

    Example location:
    ./deploy/standard/.azure/[environment_name]/.env
#>

Param(
   [parameter(Mandatory = $false, HelpMessage = "Azure EntraID Tenant")][string]$tenantID = $null,
   [string]$adminGroupName = 'FLLM-Admins',
   [string]$userGroupName = 'FLLM-Users',
   [string]$authAppName = 'FoundationaLLM-Authorization-API',
   [string]$coreAppName = 'FoundationaLLM-Core-API',
   [string]$coreClientAppName = 'FoundationaLLM-Core-Portal',
   [string]$mgmtAppName = 'FoundationaLLM-Management-API',
   [string]$mgmtClientAppName = 'FoundationaLLM-Management-Portal',
   [string]$readerAppName = 'FoundationaLLM-Reader'
)

$TranscriptName = $($MyInvocation.MyCommand.Name) -replace ".ps1", ".transcript.txt"
Start-Transcript -path .\$TranscriptName -Force

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$ScriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Path
. $ScriptDirectory/Function-Library.ps1

# Retrieve the tenant ID from the current Azure account context if not provided
if (-not $tenantID) {
   $tenantID = (az account show --query 'tenantId' --output tsv)
   if (-not $tenantID) {
      throw "Unable to retrieve tenant ID from the current Azure account context. Please provide a tenant ID."
   }
}

# Set the environment values
Write-Host -ForegroundColor Blue "Please wait while gathering azd environment values for the ${tenantID} EntraID Tenant."

try {

   $adminGroupId = $null
   Invoke-CliCommand "Get Admin Group ID" {
      $script:adminGroupId = az ad group list `
         --filter "displayName eq '$adminGroupName'" `
         --query "[0].id" `
         --output tsv
   }

   $userGroupId = $null
   Invoke-CliCommand "Get User Group ID" {
      $script:userGroupId = az ad group list `
         --filter "displayName eq '$userGroupName'" `
         --query "[0].id" `
         --output tsv
   }

   $appIds = @{}
   $appNames = @{
      auth             = $authAppName
      coreapi          = $coreAppName
      coreclient       = $coreClientAppName
      managmentapi     = $mgmtAppName
      managementclient = $mgmtClientAppName
      reader           = $readerAppName
   }

   $appData = @{}
   foreach ($app in $appNames.GetEnumerator()) {
      $data = $null
      Invoke-CliCommand "Get App ID for $($app.Value)" {
         $script:data = $(az ad app list `
            --display-name "$($app.Value)" `
            --query '[].{appId:appId,objectId:id}' `
            --output json | ConvertFrom-Json)
      }
      $appData[$app.Key] = $script:data
   }

   $values = @{
      "ADMIN_GROUP_OBJECT_ID"          = $adminGroupId
      "USER_GROUP_OBJECT_ID"           = $userGroupId
      "ENTRA_AUTH_API_CLIENT_ID"       = $appData["auth"].appId
      "ENTRA_AUTH_API_INSTANCE"        = "https://login.microsoftonline.com/"
      "ENTRA_AUTH_API_SCOPES"          = "api://FoundationaLLM-Authorization"
      "ENTRA_AUTH_API_TENANT_ID"       = $tenantID
      "ENTRA_CHAT_UI_CLIENT_ID"        = $appData["coreclient"].appId
      "ENTRA_CHAT_UI_SCOPES"           = "api://FoundationaLLM-Core/Data.Read"
      "ENTRA_CORE_API_CLIENT_ID"       = $appData["coreapi"].appId
      "ENTRA_CORE_API_SCOPES"          = "Data.Read"
      "ENTRA_MANAGEMENT_API_CLIENT_ID" = $appData["managmentapi"].appId
      "ENTRA_MANAGEMENT_API_SCOPES"    = "Data.Manage"
      "ENTRA_MANAGEMENT_UI_CLIENT_ID"  = $appData["managementclient"].appId
      "ENTRA_MANAGEMENT_UI_SCOPES"     = "api://FoundationaLLM-Management/Data.Manage"
      "ENTRA_READER_CLIENT_ID"         = $appData["reader"].appId
   }

   # Show azd environments
   $message = @"
Your azd environments are listed. Environment values updated for the default
environment file located in the .azure directory.
"@
   Write-Host -ForegroundColor Blue $message
   Invoke-CliCommand "azd env list" {
      azd env list
   }

   # Write AZD environment values
   Write-Host -ForegroundColor Yellow "Setting azd environment values for the ${tenantID} EntraID Tenant."
   foreach ($value in $values.GetEnumerator()) {
      Write-Host -ForegroundColor Yellow  "Setting $($value.Name) to $($value.Value)"
      azd env set $value.Name $value.Value
   }

   $message = @"
Environment values updated for the default environment file located in the
.azure directory.
Here are your current environment values:
"@
   Write-Host -ForegroundColor Blue $message
   Invoke-CliCommand "azd env get-values" {
      azd env get-values
   }
}
finally {
   Stop-Transcript
}
