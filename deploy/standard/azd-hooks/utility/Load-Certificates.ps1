#! /usr/bin/env pwsh
<#
.SYNOPSIS
    Loads PFX certificates into an Azure Key Vault.

.PARAMETER certificates
    An array of certificate folder names containing the PFX files to be loaded.

.PARAMETER keyVaultName
    The name of the Azure Key Vault where the certificates will be imported.

.DESCRIPTION
    This script loads PFX certificates from specified folders into an Azure Key Vault.
    It prompts the user for the password of each PFX file and imports the certificates
    using the Azure CLI. If no password is provided, the certificate is imported without a password.

.NOTES
    - The script sets strict mode to version 3.0 and stops on any error.
    - Debugging is disabled by default.
    - Utility functions are loaded from 'Load-Utility-Functions.ps1'.
    - The script expects the certificates to be located in the '../certs' directory relative to the script location.

.EXAMPLE
    .\Load-Certificates.ps1 -certificates @("cert1", "cert2") -keyVaultName "MyKeyVault"

    This command loads the certificates from the 'cert1' and 'cert2' folders into the 'MyKeyVault' Azure Key Vault.
#>

param(
    [parameter(Mandatory = $true)][array]$certificates,
    [parameter(Mandatory = $true)][string]$keyVaultName
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)

. ./utility/Load-Utility-Functions.ps1

$directories = @{
    "certs" = "../certs"
}

$pw = @{}

foreach ($certificateFolder in $certificates) {
    $pfxPath = Join-Path $directories["certs"] $certificateFolder
    $pfx = Get-ChildItem -Path $pfxPath -Filter *.pfx | Select-Object -First 1
    $keyName = $certificateFolder

    $password = Read-Host "Enter password for $($pfx.FullName) certificate (Enter for none): " -AsSecureString

    $pw[$certificateFolder] = @{
        "pfx"      = $pfx
        "keyName"  = $keyName
        "password" = $password
    }
}

foreach ($certificate in $pw.GetEnumerator()) {
    $password = $certificate.Value.password
    $certificateFolder = $certificate.Key
    $pfx = $certificate.Value.pfx
    $keyName = $certificate.Value.keyName

    if ($password.Length -eq 0) {
        Invoke-AndRequireSuccess "Load PFX Certificate $($certificateFolder) into Azure Key Vault" {
            az keyvault certificate import `
                --file $pfx `
                --name $keyName `
                --vault-name $keyVaultName
        }
    }
    else {
        Invoke-AndRequireSuccess "Load PFX Certificate $($certificateFolder) into Azure Key Vault" {
            az keyvault certificate import `
                --file $pfx `
                --name $keyName `
                --vault-name $keyVaultName `
                --password $password
        }
    }
}
