#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $true)][string]$resourceGroup,
    [parameter(Mandatory = $true)][string]$acrName,
    [parameter(Mandatory = $false)][bool]$dockerBuild = $true,
    [parameter(Mandatory = $false)][bool]$dockerPush = $true,
    [parameter(Mandatory = $false)][string]$dockerTag = "latest",
    [parameter(Mandatory = $false)][bool]$isWindowsMachine = $false
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function EnsureSuccess($message) {
    if ($LASTEXITCODE -ne 0) {
        Write-Host $message -ForegroundColor Red
        exit $LASTEXITCODE
    }    
}

Push-Location $($MyInvocation.InvocationName | Split-Path)
$sourceFolder = $(./Join-Path-Recursively.ps1 -pathParts .., scripts)

$message = @"
---------------------------------------------------
---------------------------------------------------
Getting info from ACR $resourceGroup/$acrName
---------------------------------------------------
"@
Write-Host $message -ForegroundColor Yellow

$acr = $(az acr show -g $resourceGroup -n $acrName -o json | ConvertFrom-Json)
EnsureSuccess "ACR $acrName not found"
$acrLoginServer = $acr.loginServer

$dockerComposeFile = "../docker/docker-compose.yml"


if ($dockerBuild) {
    $message = @"
---------------------------------------------------
Using docker compose to build & tag images.
Images will be named as $acrLoginServer/imageName:$dockerTag
---------------------------------------------------
"@
    Write-Host $message -ForegroundColor Yellow

    Push-Location $sourceFolder
    $env:TAG = $dockerTag
    $env:REGISTRY = $acrLoginServer 
    docker-compose -f $dockerComposeFile build
    EnsureSuccess "Docker build failed"
    Pop-Location
}

if ($dockerPush) {
    $message = @"
---------------------------------------------------
Pushing images to $acrLoginServer
---------------------------------------------------
"@
    Write-Host $message -ForegroundColor Yellow

    Push-Location $sourceFolder
    az acr login -n $acrName
    EnsureSuccess "Docker login failed"
    $env:TAG = $dockerTag
    $env:REGISTRY = $acrLoginServer 
    docker-compose -f $dockerComposeFile push
    EnsureSuccess "Docker push failed"
    Pop-Location
}

Pop-Location