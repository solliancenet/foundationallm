#! /usr/bin/env pwsh

param(
    [parameter(Mandatory = $true)][array]$certificates,
    [parameter(Mandatory = $true)][string]$keyVaultName,
    [parameter(Mandatory = $true)][string]$keyVaultResourceGroup # TODO remove unused parameter
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)

. ./utility/Load-Utility-Functions.ps1

$directories = @{
    "certs"  = "../certs"
}

foreach ($certificateFolder in $certificates) {
    $pfxPath = Join-Path $directories["certs"] $certificateFolder
    $pfx = Get-ChildItem -Path $pfxPath -Filter *.pfx | Select-Object -First 1
    $keyName = $certificateFolder

    $password = Read-Host "Enter password for $($pfx.FullName) certificate (Enter for none): " -AsSecureString
    if ($password.Length -eq 0)
    {
        Invoke-AndRequireSuccess "Load PFX Certificate $($certificateFolder) into Azure Key Vault" {
            az keyvault certificate import `
                --file $pfx `
                --name $keyName `
                --vault-name $keyVaultName
        }
    }
    else 
    {
        Invoke-AndRequireSuccess "Load PFX Certificate $($certificateFolder) into Azure Key Vault" {
            az keyvault certificate import `
                --file $pfx `
                --name $keyName `
                --vault-name $keyVaultName `
                --password $password
        }
    }
}
