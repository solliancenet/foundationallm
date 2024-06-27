#!/usr/bin/pwsh
<#
.SYNOPSIS
    Removes the OAuth callback URIs of the Chat UI and Management UI app registrations. This script is best 
    used after running the ./deploy/common/scripts/entraid/Create-FllmEntraApps.ps1 script to create your FLLM EntraID apps
    and deploying the appropriate Chat UI and Management UI infrastructure and services and Update-OAuthCallbackUris.ps1
    to create the OAuth callback URIs to be removed.

.DESCRIPTION
    This script removes the app registration OAuth callback URIs required for the Starter deployment.
    It retrieves the endpoint URIs of the User Portal and Management Portal, constructs the callback URIs from
    these endpoint URIs, and removes them if they exist from the Authentication configuration of the EntraID Apps
    required for the FLLM application. This script must be run after running azd provision in the 
    ./deploy/quick-start folder and Update-OAuthCallbackUris.ps1 from the ./tests/scripts folder.

    For more information on seting up the FLLM EntraID apps: : https://docs.foundationallm.ai/deployment/authentication/index.html
    
.PARAMETERS
Mandatory parameters:

Optional parameters:
    The names of the Chat UI and Management UI FLLM apps. If you are using the Create-FllmEntraApps.ps1 script, then the names of 
    the apps are hardcoded and will not need to be set. If you are not using the Create-FllmEntraApps.ps1 script, then you can use 
    the following parameters to set the names of the apps:
    - fllmChatUiNAme: The name of the FLLM Chat UI.
    - fllmMgmtUiName: The name of the FLLM Management UI.
    
.EXAMPLE
If you have run the Update-OAuthCallbackUris.ps1 script, then you can run the Remove-OAuthCallbackUris.ps1 script with no parameters:
    ./Remove-OAuthCallbackUris.ps1

If you have not run the Create-FllmEntraApps.ps1 script, then you can run the Set-AzdEnv.ps1 script with the tenant ID and the names of the apps.
You will need to update the app names with those you created manually in the EntraID portal.
    ./Set-AzdEnv.ps1 -fllmChatUiNAme "FoundationaLLM-Core-Portal" `
                     -fllmMgmtUiName "FoundationaLLM-Management-Portal"

#>

Param(
    [parameter(Mandatory = $false)][string]$fllmChatUiName = "FoundationaLLM-Core-Portal", # FLLM Chat UI
    [parameter(Mandatory = $false)][string]$fllmMgmtUiName = "FoundationaLLM-Management-Portal" # FLLM Management UI
)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Import azd env variables
(azd env get-values) | foreach {
    $name, $value = $_.split('=')
    set-content env:\$name $value
}

# Set the environment values
$fllmChatUiObjectId = (az ad app list --display-name "$fllmChatUiName" --query "[].{id:id,displayName:displayName}[?displayName=='$fllmChatUiName'].id" --output tsv) # FLLM Chat UI
$fllmMgmtUiObjectId = (az ad app list --display-name "$fllmMgmtUiName" --query "[].{id:id,displayName:displayName}[?displayName=='$fllmMgmtUiName'].id" --output tsv) # FLLM Management UI

# Update app registrations
$uris = @{
    "chat-ui" = @{
        endpoint = $env:SERVICE_CHAT_UI_ENDPOINT_URL
        objectId = $fllmChatUiObjectId
        query = "spa.redirectUris"
    }
    "management-ui" = @{
        endpoint = $env:SERVICE_MANAGEMENT_UI_ENDPOINT_URL
        objectId = $fllmMgmtUiObjectId
        query = "spa.redirectUris"
    }
}

foreach ($uri in $uris.GetEnumerator()) {
    if ($uri.Value.endpoint -ne $null)
    {
        $applicationUri = "https://graph.microsoft.com/v1.0/applications/" + $uri.Value.objectId
        $redirects = @(az rest `
            --method "get" `
            --uri $applicationUri `
            --headers "{'Content-Type': 'application/json'}" `
            --query $uri.Value.query `
            -o json | ConvertFrom-Json)

        $redirect = ($uri.Value.endpoint | ConvertFrom-Json) + "/signin-oidc"

        if ($redirects.Contains($redirect)) {
            $redirects -= $redirect

            $body = @{
                spa = @{
                    redirectUris = $redirects
                }
            } | ConvertTo-Json -Compress

            Set-Content -Path "$($uri.Key)`.json" $body
            az rest `
                --method "patch" `
                --uri $applicationUri `
                --headers "{'Content-Type': 'application/json'}" `
                --body "@$($uri.Key)`.json"
        }
    }
}