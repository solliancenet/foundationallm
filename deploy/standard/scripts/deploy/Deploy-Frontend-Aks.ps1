#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $false)][string]$aksName,
    [parameter(Mandatory = $false)][string]$charts = "*",
    [parameter(Mandatory = $false)][string]$ingressNginxValues,
    [parameter(Mandatory = $false)][string]$resourceGroup,
    [parameter(Mandatory = $false)][string]$secretProviderClassManifest,
    [parameter(Mandatory = $false)][string]$serviceNamespace = "fllm",
    [parameter(Mandatory = $false)][string]$registry = "ghcr.io/solliancenet",
    [parameter(Mandatory = $false)][string]$version = "0.7.0"
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
    az aks get-credentials --name $aksName --resource-group $resourceGroup --overwrite-existing
}

# **** Service Namespace ****
$serviceNamespaceYaml = @"
apiVersion: v1
kind: Namespace
metadata:
  name: ${serviceNamespace}
"@
Invoke-AndRequireSuccess "Create ${serviceNamespace} namespace" {
    $serviceNamespaceYaml | kubectl apply --filename -
}

$chartNames = @{
    "chat-ui"       = "../config/helm/chatui-values.yml"
    "management-ui" = "../config/helm/managementui-values.yml"
}
$chartsToInstall = $chartNames | Where-Object { $charts.Contains("*") -or $charts.Contains($_) }
foreach ($chart in $chartsToInstall.GetEnumerator()) {
    Invoke-AndRequireSuccess "Deploying chart $($chart.Key)" {
        $releaseName =$chart.Key
        $valuesFile = Resolve-Path $chart.Value

        helm upgrade `
            --version $version `
            --install $releaseName oci://$($registry)/helm/$($chart.Key) `
            --namespace ${serviceNamespace} `
            --values $valuesFile `
            --set image.repository=$($registry)/$($chart.Key) `
            --set image.tag=$version
    }
}

# **** Gateway Namespace ****
$gatewayNamespace = "gateway-system"
$gatewayNamespaceYaml = @"
apiVersion: v1
kind: Namespace
metadata:
  name: ${gatewayNamespace}
"@
Invoke-AndRequireSuccess "Create ${gatewayNamespace} namespace" {
    $gatewayNamespaceYaml | kubectl apply --filename -
}

Invoke-AndRequireSuccess "Deploying secret provider class" {
    kubectl apply `
        --filename=${secretProviderClassManifest} `
        --namespace=${gatewayNamespace}
}

Invoke-AndRequireSuccess "Deploy ingress-nginx" {
    helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
    helm repo update
    helm upgrade `
        --install gateway ingress-nginx/ingress-nginx `
        --namespace ${gatewayNamespace} `
        --values ${ingressNginxValues} `
        --version 4.10.0
}

Start-Sleep -Seconds 60

$ingressNames = @{
    "chat-ui"       = "../config/helm/chatui-ingress.yml"
    "management-ui" = "../config/helm/managementui-ingress.yml"
}
foreach ($ingress in $ingressNames.GetEnumerator()) {
    Invoke-AndRequireSuccess "Deploying ingress for $($ingress.Key)" {
        $ingressFile = Resolve-Path $ingress.Value
        kubectl apply --filename $ingressFile --namespace ${gatewayNamespace}
    }
}