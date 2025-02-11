#!/usr/bin/env pwsh

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Load utility functions
Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    . ./utility/Load-Utility-Functions.ps1
}
finally {
    Pop-Location
}

Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    Invoke-AndRequireSuccess "Download AzCopy for the FoundationaLLM solution" {
        Push-Location ../../common/scripts
        ./Get-AzCopy.ps1
        Pop-Location
    }
}
finally {
    Pop-Location
}

# # Navigate to the script directory so that we can use relative paths.
# Push-Location $($MyInvocation.InvocationName | Split-Path)
# try {
#     # Map TLS certifiates to secret names
#     $certificates = @(
#         "chatui",
#         "coreapi",
#         "managementui",
#         "managementapi"
#     )

#     Invoke-AndRequireSuccess "Load Certificates" {
#         ./utility/Load-Certificates.ps1 `
#             -keyVaultResourceGroup $env:FLLM_OPS_RG `
#             -keyVaultName $env:FLLM_OPS_KV `
#             -certificates $certificates
#     }
# }
# finally {
#     Pop-Location
# }

Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    if ($env:EXTENSIONS_INSTALLED) {
        $extensions = @("aks-preview", "application-insights", "storage-preview", "eventgrid")
        foreach ($extension in $extensions) {
            Invoke-AndRequireSuccess "Install $extension extension" {
                az extension add --name $extension --allow-preview true --yes
                az extension update --name $extension --allow-preview true
            }
        }

        # TODO this needs to be in its own try-finally block
        Push-Location ..
        azd env set EXTENSIONS_INSTALLED 1
        Pop-Location
    }

    # Convert the manifest resource groups to a hashtable for easier access
    $resourceGroup = @{
        app     = $env:FLLM_APP_RG
        auth    = $env:FLLM_AUTH_RG
        data    = $env:FLLM_DATA_RG
        dns     = $env:FLLM_DNS_RG
        jbx     = $env:FLLM_JBX_RG
        net     = $env:FLLM_NET_RG
        oai     = $env:FLLM_OAI_RG
        ops     = $env:FLLM_OPS_RG
        storage = $env:FLLM_STORAGE_RG
        vec     = $env:FLLM_VEC_RG
    }

    $entraClientIds = @{
        authorization = $env:ENTRA_AUTH_API_CLIENT_ID
        chat          = $env:ENTRA_CHAT_UI_CLIENT_ID
        core          = $env:ENTRA_CORE_API_CLIENT_ID
        managementapi = $env:ENTRA_MANAGEMENT_API_CLIENT_ID
        managementui  = $env:ENTRA_MANAGEMENT_UI_CLIENT_ID
    }

    $entraScopes = @{
        authorization = "api://FoundationaLLM-Authorization/.default"
        chat          = "api://FoundationaLLM-Core/Data.Read"
        core          = "Data.Read"
        managementapi = "Data.Manage"
        managementui  = "api://FoundationaLLM-Management/Data.Manage"
    }

    # Get frontend and backend hostnames
    $ingress = @{
        apiIngress = @{}
        frontendIngress = @{}
    }

    $frontEndHosts = @()
    if ($env:FLLM_USER_PORTAL_HOSTNAME) {
        $frontEndHosts += $env:FLLM_USER_PORTAL_HOSTNAME
        $($ingress.frontendIngress).chatui = @{
            host = $env:FLLM_USER_PORTAL_HOSTNAME
            path = "/"
            pathType = "ImplementationSpecific"
            serviceName = "chat-ui"
            sslCert = "chatui"
        }
    }

    if ($env:FLLM_MGMT_PORTAL_HOSTNAME) {
        $frontEndHosts += $env:FLLM_MGMT_PORTAL_HOSTNAME
        $($ingress.frontendIngress).managementui = @{
            host = $env:FLLM_MGMT_PORTAL_HOSTNAME
            path = "/"
            pathType = "ImplementationSpecific"
            serviceName = "management-ui"
            sslCert = "managementui"
        }
    }

    $backendHosts = @()
    if ($env:FLLM_CORE_API_HOSTNAME) {
        $backendHosts += $env:FLLM_CORE_API_HOSTNAME
        $($ingress.apiIngress).coreapi = @{
            host = $env:FLLM_CORE_API_HOSTNAME
            path = "/core/"
            pathType = "ImplementationSpecific"
            serviceName = "core-api"
            sslCert = "coreapi"
        }
    }

    if ($env:FLLM_MGMT_API_HOSTNAME) {
        $backendHosts += $env:FLLM_MGMT_API_HOSTNAME
        $($ingress.apiIngress).managementapi = @{
            host = $env:FLLM_MGMT_API_HOSTNAME
            path = "/management/"
            pathType = "ImplementationSpecific"
            serviceName = "management-api"
            sslCert = "managementapi"
        }
    }

    Invoke-AndRequireSuccess "Generate Configuration" {
        ./utility/Generate-Config.ps1 `
            -adminGroupObjectId $env:ADMIN_GROUP_OBJECT_ID `
            -userGroupObjectId $env:USER_GROUP_OBJECT_ID `
            -entraClientIds $entraClientIds `
            -entraScopes $entraScopes `
            -instanceId $env:FOUNDATIONALLM_INSTANCE_ID `
            -resourceGroups $resourceGroup `
            -resourceSuffix $(Get-Resource-Suffix) `
            -serviceNamespaceName $env:FOUNDATIONALLM_K8S_NS `
            -subscriptionId $env:AZURE_SUBSCRIPTION_ID `
            -ingress $ingress
    }

    $appConfigName = Invoke-AndRequireSuccess "Get AppConfig" {
        az appconfig list `
            --resource-group $($resourceGroup.ops) `
            --query "[0].name" `
            --output tsv
    }

    $configurationFile = Resolve-Path "../config/appconfig.json"
    Invoke-AndRequireSuccess "Loading AppConfig Values" {
        az appconfig kv import `
            --profile appconfig/kvset `
            --name $appConfigName `
            --source file `
            --path $configurationFile `
            --format json `
            --yes `
            --output none
    }

    ./utility/Upload-AuthStoreData.ps1 `
        -resourceGroup $resourceGroup["auth"] `
        -instanceId $env:FOUNDATIONALLM_INSTANCE_ID

    ./utility/Upload-SystemPrompts.ps1 `
        -resourceGroup $resourceGroup["storage"] `
        -location $env:AZURE_LOCATION

    $backendAks = Invoke-AndRequireSuccess "Get Backend AKS" {
        az aks list `
            --resource-group $($resourceGroup.app) `
            --query "[?contains(name, 'backend')].name | [0]" `
            --output tsv
    }

    $secretProviderClassManifestBackend = Resolve-Path "../config/kubernetes/spc.foundationallm-certificates.backend.yml"
    $ingressNginxValuesBackend = Resolve-Path "../config/helm/ingress-nginx.values.backend.yml"
    Invoke-AndRequireSuccess "Deploy Backend" {
        ./utility/Deploy-Backend-Aks.ps1 `
            -aksName $backendAks `
            -ingressNginxValues $ingressNginxValuesBackend `
            -resourceGroup $resourceGroup.app `
            -secretProviderClassManifest $secretProviderClassManifestBackend `
            -serviceNamespace $env:FOUNDATIONALLM_K8S_NS `
            -registry $env:FOUNDATIONALLM_REGISTRY `
            -version $env:FLLM_VERSION
    }

    $frontendAks = Invoke-AndRequireSuccess "Get Frontend AKS" {
        az aks list `
            --resource-group $($resourceGroup.app) `
            --query "[?contains(name, 'frontend')].name | [0]" `
            --output tsv
    }

    $secretProviderClassManifestFrontend = Resolve-Path "../config/kubernetes/spc.foundationallm-certificates.frontend.yml"
    $ingressNginxValuesFrontend = Resolve-Path "../config/helm/ingress-nginx.values.frontend.yml"
    Invoke-AndRequireSuccess "Deploy Frontend" {
        ./utility/Deploy-Frontend-Aks.ps1 `
            -aksName $frontendAks `
            -ingressNginxValues $ingressNginxValuesFrontend `
            -resourceGroup $resourceGroup.app `
            -secretProviderClassManifest $secretProviderClassManifestFrontend `
            -serviceNamespace $env:FOUNDATIONALLM_K8S_NS `
            -registry $env:FOUNDATIONALLM_REGISTRY `
            -version $env:FLLM_VERSION
    }

    $clusters = @(
        @{
            cluster = $frontendAks
            hosts   = $frontEndHosts
        }
        @{
            cluster = $backendAks
            hosts   = $backendHosts
        }
    )
    Invoke-AndRequireSuccess "Generate AKS Ingress Host Entires" {
        ./utility/Generate-Ingress-Hosts.ps1 `
            -resourceGroup $resourceGroup.app `
            -clusters $clusters
    }
}
finally {
    Pop-Location
}
