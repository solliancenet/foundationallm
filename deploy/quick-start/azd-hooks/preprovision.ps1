#!/usr/bin/env pwsh

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

Write-Host "Fetching FLLM Version..." -ForegroundColor Blue
$fllmVersionConfigPath = "./config/version.json"
$fllmVersionConfig = (Get-content $fllmVersionConfigPath | ConvertFrom-Json)

Write-Host "Setting FLLM Version to $($fllmVersionConfig.version)..." -ForegroundColor Blue
azd env set FLLM_VERSION "$($fllmVersionConfig.version)"

$instanceId = $(azd env get-value FOUNDATIONALLM_INSTANCE_ID)
if ($LastExitCode -eq 0) 
{
    # TODO: Validate that it is a proper GUID
}
else
{
    $instanceId = $((New-Guid).Guid)
    azd env set FOUNDATIONALLM_INSTANCE_ID $instanceId
}