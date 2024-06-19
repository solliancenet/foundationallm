#!/usr/bin/env pwsh

<#
.SYNOPSIS
    This script runs the pre-deployment tasks for the FoundationAllM solution.

.DESCRIPTION
    This script performs necessary pre-deployment tasks such as loading deployment manifests,
    generating certificates, and creating required Azure AD app registrations and admin groups.
    Depending on the customer setup, some tasks can be skipped.

.EXAMPLE
    Pre-Deploy.ps1 -manifestName "Deployment-Manifest.json" -skipCertificates $true
#>

param(
    [parameter(Mandatory = $false)][string]$manifestName = "Deployment-Manifest.json",
    [parameter(Mandatory = $false)][bool]$skipCertificates = $false,
    [parameter(Mandatory = $false)][bool]$skipEntraIdApps = $false,
    [parameter(Mandatory = $false)][bool]$skipEntraIdAdminGroup = $false,
    [parameter(Mandatory = $false)][bool]$skipGetAzCopy = $false
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Load the Invoke-AndRequireSuccess function
. ./utility/Invoke-AndRequireSuccess.ps1

# Navigate to the script directory so that we can use relative paths.
Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    Write-Host "Loading Deployment Manifest ../${manifestName}" -ForegroundColor Blue
    $manifest = $(Get-Content -Raw -Path ../${manifestName} | ConvertFrom-Json)

    if (-not $skipCertificates) {
        Invoke-AndRequireSuccess "Generate Certificates" {
            ./pre-provision/Get-LetsEncryptCertificates.ps1 `
                -baseDomain $manifest.baseDomain `
                -email $manifest.letsEncryptEmail `
                -subdomainPrefix $manifest.project
        }
    }
    else {
        Write-Host "Skipping certificate generation as per the skipCertificates parameter." -ForegroundColor Yellow
    }
    if (-not $skipEntraIdApps) {
        Invoke-AndRequireSuccess "Create FLLM EntraID App Registrations" {
            ./pre-provision/Create-FllmEntraIdApps.ps1
        }
    }
    else {
        Write-Host "Skipping EntraID app registration creation as per the skipEntraIdApps parameter." -ForegroundColor Yellow
    }
    if (-not $skipEntraIdAdminGroup) {
        Invoke-AndRequireSuccess "Create FLLM Admin Group" {
            ./pre-provision/Create-FllmAdminGroup.ps1
        }
    }
    else {
        Write-Host "Skipping EntraID admin group creation as per the skipEntraIdAdminGroup parameter." -ForegroundColor Yellow
    }
    if (-not $skipGetAzCopy) {
        Invoke-AndRequireSuccess "Download AzCopy for the FoundationaLLM solution" {
            ./pre-provision/Get-AzCopy.ps1
        }
    }
    else {
        Write-Host "Skipping getAzCopy as per the skipGetAzCopy parameter." -ForegroundColor Yellow
    }
}
finally {
    Pop-Location
}
