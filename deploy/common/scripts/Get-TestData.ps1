#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $true)][string]$storageAccountName
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Invoke-CLICommand {
    <#
    .SYNOPSIS
    Invoke a CLI Command and allow all output to print to the terminal.  Does not check for return values or pass the output to the caller.
    #>
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    Write-Host "${message}..." -ForegroundColor Blue
    & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        throw "Failed ${message} (code: ${LASTEXITCODE})"
    }
}

function Get-AbsolutePath {
    <#
    .SYNOPSIS
    Get the absolute path of a file or directory. Relative path does not need to exist.
    #>
    param (
        [Parameter(Mandatory = $true, ValueFromPipeline = $true, Position = 0)]
        [string]$RelatviePath
    )

    return $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($RelatviePath)
}

# Navigate to the script directory so that we can use relative paths.
Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    $testDataPath = "../data/test" | Get-AbsolutePath
    Write-Host "Writing vectorization-input data to: $testDataPath" -ForegroundColor Yellow
    Invoke-CLICommand "Copy vectorization-input data from the storage account: ${storageAccountName}" {
        azcopy copy "https://$($storageAccountName).blob.core.windows.net/e2e/vectorization-input" $testDataPath --recursive
    }

    Write-Host "Writing index-backup data to: $testDataPath" -ForegroundColor Yellow
    Invoke-CLICommand "Copy index-backup data from the storage account: ${storageAccountName}" {
        azcopy copy "https://$($storageAccountName).blob.core.windows.net/e2e/index-backup" $testDataPath --recursive
    }

    $testSettingsPath = "../../../tests/dotnet/Core.Examples/testsettings.json" | Get-AbsolutePath
    Write-Host "Writing testsettings.json data to: $testSettingsPath" -ForegroundColor Yellow
    Invoke-CLICommand "Copy testsettings.json data from the storage account: ${storageAccountName}" {
        azcopy copy "https://$($storageAccountName).blob.core.windows.net/e2e/testsettings.json" $testSettingsPath
    }
}
catch {
    $logFile = Get-ChildItem -Path "$env:HOME/.azcopy" -Filter "*.log" | `
        Where-Object { $_.Name -notlike "*-scanning*" } | `
        Sort-Object LastWriteTime -Descending | `
        Select-Object -First 1
    $logFileContent = Get-Content -Raw -Path $logFile.FullName
    Write-Host $logFileContent
}
finally {
    Pop-Location
    Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
}
