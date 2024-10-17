#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $false)][string]$charts = "*",
    [parameter(Mandatory = $false)][string]$loginServer = "ghcr.io/solliancenet/foundationallm",
    [parameter(Mandatory = $false)][string]$serviceNamespace = "fllm",
    [parameter(Mandatory = $false)][string]$updateVersion = "0.4.1",
    [parameter(Mandatory = $false)][string]$version = "0.4.1",
    [parameter(Mandatory = $true)][hashtable]$chartNames,
    [parameter(Mandatory = $true)][string]$aksName,
    [parameter(Mandatory = $true)][string]$resourceGroup
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Invoke-AndRequireSuccess {
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    Write-Host "${message}..." -ForegroundColor Blue
    $result = & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        throw "Failed ${message} (code: ${LASTEXITCODE})"
    }

    return $result
}

Invoke-AndRequireSuccess "Retrieving credentials for AKS cluster ${aksName}" {
    az aks get-credentials `
        --name $aksName `
        --resource-group $resourceGroup `
        --overwrite-existing
}

$chartsToInstall = $chartNames | Where-Object { $charts.Contains("*") -or $charts.Contains($_) }
foreach ($chart in $chartsToInstall.GetEnumerator()) {
    Invoke-AndRequireSuccess "Updating chart $($chart.Key)" {
        $releaseName = $chart.Key
        $valuesFile = Resolve-Path $chart.Value.values
        $chartPath = Resolve-Path "../../common/helm/$($chart.Key)"
        # oci://ghcr.io/solliancenet/foundationallm/helm/$($chart.Key)

        helm upgrade `
            --reuse-values `
            --version $version `
            --install $releaseName $chartPath `
            --namespace ${serviceNamespace} `
            --values $valuesFile `
            --set image.repository=$($loginServer)/$($chart.Value.image) `
            --set image.tag=$updateVersion
    }
}
