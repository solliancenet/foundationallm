#! /usr/bin/env pwsh

param(
    [parameter(Mandatory = $true)][object]$certificates,
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

foreach ($certificate in $certificates.GetEnumerator()) {
    $pfxPath = Join-Path $directories["certs"] $certificate.Key
    $pfx = Get-ChildItem -Path $pfxPath -Filter *.pfx | Select-Object -First 1
    $keyName = $certificate.Key

    Invoke-AndRequireSuccess "Load PFX Certificate $($certificate.Key) into Azure Key Vault" {
        az keyvault certificate import `
            --file $pfx.FullName `
            --name $keyName `
            --vault-name $keyVaultName `
            --password "$($certificate.Value)"
    }
}
