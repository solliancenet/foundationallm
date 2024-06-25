#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $true)][string]$aiSearchName,
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

function Set-Index {
    <#
    .SYNOPSIS
    Restore an Azure AI Search Index from an local file.
    #>
    param (
        [Parameter(Mandatory = $true)]
        [string]$aiSearchName,
        [Parameter(Mandatory = $true)]
        [string]$indexBackupPath,
        [Parameter(Mandatory = $true)]
        [string]$originalIndexName,
        [Parameter(Mandatory = $true)]
        [string]$restoreIndexName
    )

    $token = (az account get-access-token --scope https://search.azure.com/.default --query accessToken --output tsv)
    $headers = @{
        Authorization  = "Bearer $token"
        "Content-Type" = "application/json"
    }

    # Get schema from the storage account
    $schemaFileName = $indexBackupPath | join-path -ChildPath $originalIndexName -AdditionalChildPath "${originalIndexName}.schema"
    Write-Host "Reading schema from: $schemaFileName" -ForegroundColor Yellow

    $schemaObject = Get-Content -Raw -Path $schemaFileName | ConvertFrom-Json -Depth 9
    $schemaObject.name = $restoreIndexName
    $schemaContent = $schemaObject | ConvertTo-Json -Depth 9

    # Create the index, delete first if it already exists
    $searchAPIVersion = "2023-11-01"
    $serviceUri = "https://$($aiSearchName).search.windows.net"
    $indexUri = $serviceUri, "indexes", $restoreIndexName, "?api-version=$searchAPIVersion" | Join-String -Separator "/"
    Write-Host "Creating new index: $indexUri" -ForegroundColor Yellow

    try {
        $existingIndex = Invoke-RestMethod -Uri $indexUri -Headers $headers -Method Get
        if ($existingIndex) {
            Invoke-RestMethod -Uri $indexUri -Headers $headers -Method Delete
            Write-Host "Deleted existing index: $restoreIndexName"
        }
    }
    catch [Microsoft.PowerShell.Commands.HttpResponseException] {
        # If the index does not exist, the StatusCode will be NotFound, and we
        # do not rethrow, we can proceed to create the index
        $response = $_.Exception.Response
        if ($response.StatusCode -ne [System.Net.HttpStatusCode]::NotFound) {
            throw
        }
    }

    Invoke-RestMethod -Uri $indexUri -Headers $headers -Method Put -Body $schemaContent
    Write-Host "Created new index: $restoreIndexName"

    # Read backup files from local storage and upload to the new index
    $sourcePath = $indexBackupPath | join-path -ChildPath $originalIndexName -AdditionalChildPath "${originalIndexName}_*.json"
    Write-Host "Reading backup index from: $sourcePath" -ForegroundColor Yellow
    $blobs = Get-ChildItem -Path $sourcePath | Sort-Object Name
    foreach ($blob in $blobs) {
        $blobObject = Get-Content -Raw -Path $blob.FullName | ConvertFrom-Json -Depth 9
        $documentArray = $blobObject.value
        foreach ($document in $documentArray) {
            $document.PSObject.Properties.Remove('@search.score')
            $document | Add-Member -MemberType NoteProperty -Name '@search.action' -Value 'mergeOrUpload'
        }

        $documentPayload = @{ value = $documentArray } | ConvertTo-Json -Depth 9
        $uri = $serviceUri, "indexes", $restoreIndexName, "docs", "index?api-version=$searchAPIVersion" | Join-String -Separator "/"
        write-host "Uplodaing backup page to $uri"
        Invoke-RestMethod -Uri $uri -Headers $headers -Method Post -Body $documentPayload
        Write-Host "Uploaded backup page to $restoreIndexName"
    }
    Write-Host "Restored index $restoreIndexName from backup." -ForegroundColor Blue
}

# Navigate to the script directory so that we can use relative paths.
Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    $indexBackupPath = "../data/test/index-backup" | Get-AbsolutePath
    Write-Host "Reading index-backup data from: $indexBackupPath" -ForegroundColor Yellow

    $indexBackupDirectories = Get-ChildItem -Path $indexBackupPath -Directory
    foreach ($indexBackupDirectory in $indexBackupDirectories) {
        $indexBackupDirectoryName = $indexBackupDirectory.Name
        Write-Host "Restoring index backup: $indexBackupDirectoryName" -ForegroundColor Yellow
        Set-Index -aiSearchName $aiSearchName `
            -indexBackupPath $indexBackupPath `
            -originalIndexName $indexBackupDirectoryName `
            -restoreIndexName $indexBackupDirectoryName
    }

    $testDataPath = "../data/test/vectorization-input" | Get-AbsolutePath | join-path -ChildPath "*"
    Write-Host "Reading vectorization-input data from: $testDataPath" -ForegroundColor Yellow
    Invoke-CLICommand "Copy vectorization-input data to the storage account ${storageAccountName}" {
        azcopy copy "$testDataPath" "https://$($storageAccountName).blob.core.windows.net/vectorization-input" --recursive
    }
}
catch {
    $logFile = Get-ChildItem -Path "$env:HOME/.azcopy" -Filter "*.log" | `
        Where-Object { $_.Name -notlike "*-scanning*" } | `
        Sort-Object LastWriteTime -Descending | `
        Select-Object -First 1
    $logFileContent = Get-Content -Raw -Path $logFile.FullName
    Write-Host $logFileContent
    throw
}
finally {
    Pop-Location
    Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
}
