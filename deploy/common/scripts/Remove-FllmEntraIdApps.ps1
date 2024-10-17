#! /usr/bin/pwsh
<#
.SYNOPSIS
    Automates the deletion of specific Entra ID applications based on their display names.

.DESCRIPTION
    This script is designed to remove Entra applications by their display names. It allows the user to specify custom application names 
    or use the default ones provided. The script supports an interactive mode that prompts the user for confirmation before deletion.
    This ensures that critical applications are not accidentally removed. The script is useful for managing and cleaning up Entra 
    environments by removing outdated or unused applications.

.PARAMETER authAppName
    The display name of the Authorization API application to be deleted. Default is "FoundationaLLM-Authorization-API".

.PARAMETER coreAppName
    The display name of the Core API application to be deleted. Default is "FoundationaLLM-Core-API".

.PARAMETER coreClientAppName
    The display name of the Core Portal application to be deleted. Default is "FoundationaLLM-Core-Portal".

.PARAMETER mgmtAppName
    The display name of the Management API application to be deleted. Default is "FoundationaLLM-Management-API".

.PARAMETER mgmtClientAppName
    The display name of the Management Portal application to be deleted. Default is "FoundationaLLM-Management-Portal".

.PARAMETER mgmtClientAppName
    The display name of the Reader application to be deleted. Default is "FoundationaLLM-Reader".

.PARAMETER interactiveMode
    Boolean flag to determine if the script should run in interactive mode, prompting for user confirmation before deletion. 
    Default is $true.

.EXAMPLE
    ./Remove-FllmEntraIdApps.ps1
    This example runs the script to delete the default Entra applications prompting for confirmation.

.NOTES
	This is a destructive script. Use with caution and ensure that the applications to be deleted are no longer needed.
	The script requires the Azure CLI to be installed and authenticated and the subscription with the Entra Set using
	the 'az account set --subscription' command.
#>

Param(
    [parameter(Mandatory = $false)][string]$authAppName="FoundationaLLM-Authorization-API",
    [parameter(Mandatory = $false)][string]$coreAppName="FoundationaLLM-Core-API",
    [parameter(Mandatory = $false)][string]$coreClientAppName="FoundationaLLM-Core-Portal",
    [parameter(Mandatory = $false)][string]$mgmtAppName="FoundationaLLM-Management-API",
    [parameter(Mandatory = $false)][string]$mgmtClientAppName="FoundationaLLM-Management-Portal",
	[parameter(Mandatory = $false)][string]$mgmtReaderAppName = "FoundationaLLM-Reader",
	[parameter(Mandatory=$false)][bool]$interactiveMode = $true
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Predefined list of application names to delete
$AppNames = @(
	$authAppName, 
	$coreAppName, 
	$coreClientAppName,
	$mgmtAppName, 
	$mgmtClientAppName,
	$mgmtReaderAppName
)

# Function to filter and delete Azure AD applications based on display name
function Delete-AppByName {
	param(
		[string]$Name,
		[bool]$interactiveMode
	)

	# Find applications matching the name
	$appIds = az ad app list --all --query "[].{appId:appId,displayName:displayName}[?displayName=='$Name'].appId" --output tsv

	if (-not $appIds) {
		Write-Host -ForegroundColor Red "No applications found with name: $Name"
		return
	}

	# Display warning message and ask for confirmation
	Write-Host -ForegroundColor Blue "Application to be deleted with name '$Name':"
	az ad app list --all --query "[].{displayName:displayName}[?displayName=='$Name'].displayName" --output table

	if ($interactiveMode) {
		$confirmation = Read-Host "Are you sure you want to delete the application? This can't be undone (yes/no)"
	} else {
		$confirmation = "yes"
	}

	# Delete applications if confirmed
	if ($confirmation -eq "yes") {
		foreach ($appId in $appIds.Split("`n")) {
			Write-Host -ForegroundColor Yellow "Deleting Entra application with ID: $appId"
			az ad app delete --id $appId --only-show-errors
			Write-Host -ForegroundColor Green "Application with ID: $appId deleted successfully"
		}
	}
 else {
		Write-Host -ForegroundColor Red "Deletion cancelled for applications with name: $Name"
	}
}

# Process each application name provided in the list
foreach ($appName in $AppNames) {
	Delete-AppByName -Name $appName -interactiveMode $interactiveMode
}
