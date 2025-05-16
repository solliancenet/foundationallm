#!/bin/usr/env pwsh

Install-Module -Name powershell-yaml -Force
Import-Module powershell-yaml

function Expand-Tar($tarFile, $dest) {

    if (-not (Get-Command Expand-7Zip -ErrorAction Ignore)) {
        Install-Package -Scope CurrentUser -Force 7Zip4PowerShell > $null
    }

    Expand-7Zip $tarFile $dest
}

function Escrow-Docker-Image {
    param (
        [string]$service,
        [string]$version,
        [string]$tagWithSuffix,
        [string]$sourceRegistry,
        [string]$destRegistry,
        [string]$destUsername,
        [string]$destPassword
    )

    if ($tagWithSuffix) {
        $srcImageUrl = "$($sourceRegistry)/$($service):$($tagWithSuffix)"
    } else {
        $srcImageUrl = "$($sourceRegistry)/$($service):$($version)"
    }

    $destImageUrl = "$($destRegistry)/$($service):$($version)"

    docker logout
    docker pull $srcImageUrl
    docker tag $srcImageUrl $destImageUrl
    echo $destPassword | docker login $destRegistry -u $destUsername --password-stdin
    docker push $destImageUrl
}

function Get-NestedProperty {
    param (
        [object]$obj,
        [string]$propertyPath
    )

    $properties = $propertyPath -split '\.'
    foreach ($property in $properties) {
        if ($obj -is [System.Collections.IDictionary]) {
            $obj = $obj[$property]
        } else {
            $obj = $obj.$property
        }
    }
    return $obj
}

function Escrow-Helm-Chart-Dependencies {
    param (
        [string]$chartPath,
        [object]$dependencies,
        [string]$destRegistry,
        [string]$destUsername,
        [string]$destPassword
    )

    if ($dependencies.Count -eq 0) {
        Write-Host "No dependencies to escrow." -ForegroundColor Yellow
        return
    }

    Write-Host "Escrowing dependencies..." -ForegroundColor Green
    $values = Get-Content $chartPath/values.yaml | ConvertFrom-Yaml
    $globalReference = (Get-NestedProperty -obj $values -propertyPath "global.image")

    foreach ($dependency in $dependencies) {
        Write-Host "Escrowing image $($dependency.name) to $destRegistry" -ForegroundColor Green

        $reference = (Get-NestedProperty -obj $values -propertyPath $dependency.helmValue)
        $image = $reference.image
        $tag = $reference.tag
        if ($reference.PSObject.Properties.Name -contains "registry") {
            $registry = $reference.registry
        } else {
            $registry = $globalReference.registry
        }
        if ($dependency.suffix)
        {
            $suffix = "@$(Get-NestedProperty -obj $reference -propertyPath $dependency.suffix)"
            $tagWithSuffix = $tag + $suffix
        } else {
            $tagWithSuffix = $tag
        }

        $reference.registry = $destRegistry

        foreach ($entry in $dependency.remove) {
            $reference.Remove($entry)
        }

        Escrow-Docker-Image -service $image `
            -version $tag `
            -tagWithSuffix $tagWithSuffix `
            -sourceRegistry $registry `
            -destRegistry $destRegistry `
            -destUsername $destUsername `
            -destPassword $destPassword
    }

    Out-File -FilePath "$chartPath/values.yaml" -InputObject ($values | ConvertTo-Yaml) -Encoding utf8
}

function Escrow-Helm-Chart {
    param (
        [string]$service,
        [string]$version,
        [string]$helmChart,
        [string]$helmRepo = $null,
        [string]$destRegistry,
        [string]$destUsername,
        [string]$destPassword,
        [object]$dependencies
    )

    if ($helmRepo) {
        helm repo add $helmChart $helmRepo
        helm repo update
        $srcChartUrl = "$helmChart/$service"
    } else {
        $srcChartUrl = $helmChart
    }

    $destChartUrl = "oci://$($destRegistry)/helm"

    if(Test-Path ./$service) {
        Remove-Item -Path "./$service" -Recurse -Force
    }

    docker logout
    helm pull $srcChartUrl --version $version --untar

    Escrow-Helm-Chart-Dependencies -dependencies $dependencies `
        -chartPath ./$service `
        -destRegistry $destRegistry `
        -destUsername $destUsername `
        -destPassword $destPassword

    helm package $service
    echo $destPassword | docker login $destRegistry -u $destUsername --password-stdin
    helm push "$($service)-$($version).tgz" $destChartUrl
    Remove-Item -Path "$($service)-$($version).tgz" -Force
    Remove-Item -Path "./$service" -Recurse -Force
}

function Get-Escrow-Config {
    param (
    )

    $escrowImages = $(azd env get-value FOUNDATIONALLM_ESCROWED)
    if ($LastExitCode -ne 0 -or $escrowImages -eq "") {
        while ($escrowImages -ne "Y" -and $escrowImages -ne "N") {
            $escrowImages = Read-Host "Escrow container images ([Y]es, [N]o): "
            $escrowImages = $escrowImages.Trim()
        }
        
        azd env set FOUNDATIONALLM_ESCROWED $escrowImages
    } else {
        $newEscrowImages = $null
        while ($newEscrowImages -ne "Y" -and $newEscrowImages -ne "N" -and $newEscrowImages -ne "") {
            $newEscrowImages = Read-Host "Escrow container images ([Y]es, [N]o, [Enter] for $escrowImages): "
            $newEscrowImages = $newEscrowImages.Trim()
        }

        if ($newEscrowImages -ne "") {
            $escrowImages = $newEscrowImages
            azd env set FOUNDATIONALLM_ESCROWED $escrowImages
        }
    }

    Write-Host "Escrow container images: $escrowImages" -ForegroundColor Green

    if ($escrowImages -eq "Y") {
        $escrowRegistry = $(azd env get-value FOUNDATIONALLM_ESCROW_REGISTRY)
        if ($LastExitCode -ne 0) {
            $escrowRegistry = Read-Host "Escrow Registry: "
            $escrowRegistry = $escrowRegistry.Trim()
            if ($escrowRegistry -eq "") {
                throw "Escrow Registry cannot be empty."
            } else {
                azd env set FOUNDATIONALLM_ESCROW_REGISTRY $escrowRegistry
            }
        } else {
            $newEscrowRegistry = Read-Host "Escrow Registry (Enter for '$escrowRegistry'): "
            $newEscrowRegistry = $newEscrowRegistry.Trim()
            if ($newEscrowRegistry -ne "") {
                $escrowRegistry = $newEscrowRegistry
                azd env set FOUNDATIONALLM_ESCROW_REGISTRY $escrowRegistry
            }
        }

        $escrowUsername = $(azd env get-value FOUNDATIONALLM_ESCROW_USERNAME)
        if ($LastExitCode -ne 0) {
            $escrowUsername = Read-Host "Escrow Registry Username: "
            $escrowUsername = $escrowUsername.Trim()
            if ($escrowUsername -eq "") {
                throw "Escrow Registry Username cannot be empty."
            } else {
                azd env set FOUNDATIONALLM_ESCROW_USERNAME $escrowUsername
            }
        } else {
            $newEscrowUsername = Read-Host "Escrow Registry Username (Enter for '$escrowUsername'): "
            $newEscrowUsername = $newEscrowUsername.Trim()
            if ($newEscrowUsername -ne "") {
                $escrowUsername = $newEscrowUsername
                azd env set FOUNDATIONALLM_ESCROW_USERNAME $escrowUsername
            }
        }

        $escrowPassword = $(azd env get-value FOUNDATIONALLM_ESCROW_PASSWORD)
        if ($LastExitCode -ne 0) {
            $escrowPassword = Read-Host "Escrow Registry Password: "
            $escrowPassword = $escrowPassword.Trim()
            if ($escrowPassword -eq "") {
                throw "Escrow Registry Password cannot be empty."
            } else {
                azd env set FOUNDATIONALLM_ESCROW_PASSWORD $escrowPassword
            }
        } else {
            $newEscrowPassword = Read-Host "Escrow Registry Password (Enter to use existing password): "
            $newEscrowPassword = $newEscrowPassword.Trim()
            if ($newEscrowPassword -ne "") {
                $escrowPassword = $newEscrowPassword
                azd env set FOUNDATIONALLM_ESCROW_PASSWORD $escrowPassword
            }
        }

        azd env set FOUNDATIONALLM_REGISTRY $escrowRegistry
    } else {
        azd env set FOUNDATIONALLM_REGISTRY ghcr.io/solliancenet/foundationallm
    }

    return $escrowImages -eq "Y"
}

function Escrow-FoundationaLLM-Images {
    param (
        [string]$version
    )

    $json = Get-Content -Path "./config/escrow-config.json" -Raw | ConvertFrom-Json
    $services = $json.services
    $sourceRegistry = $json.sourceRegistry

    $destRegistry = $(azd env get-value FOUNDATIONALLM_ESCROW_REGISTRY)
    $destUsername = $(azd env get-value FOUNDATIONALLM_ESCROW_USERNAME)
    $destPassword = $(azd env get-value FOUNDATIONALLM_ESCROW_PASSWORD)

    foreach ($service in $services) {
        Write-Host "Escrowing image $service to $destRegistry" -ForegroundColor Green
        Escrow-Docker-Image `
            -service $service `
            -version $version `
            -sourceRegistry $sourceRegistry `
            -destRegistry $destRegistry `
            -destUsername $destUsername `
            -destPassword $destPassword
    }
}

function Escrow-FoundationaLLM-Helm-Charts {
    param (
        [string]$version
    )

    $json = Get-Content -Path "./config/escrow-config.json" -Raw | ConvertFrom-Json
    $services = $json.services
    $sourceRegistry = $json.sourceRegistry

    $destRegistry = $(azd env get-value FOUNDATIONALLM_ESCROW_REGISTRY)
    $destUsername = $(azd env get-value FOUNDATIONALLM_ESCROW_USERNAME)
    $destPassword = $(azd env get-value FOUNDATIONALLM_ESCROW_PASSWORD)


    $dependencies = @{}
    foreach ($service in $services) {
        $srcChartUrl = "oci://$sourceRegistry/helm/$service"

        Write-Host "Escrowing chart $srcChartUrl to $destRegistry" -ForegroundColor Green
        Escrow-Helm-Chart `
            -service $service `
            -version $version `
            -helmChart $srcChartUrl `
            -destRegistry $destRegistry `
            -destUsername $destUsername `
            -destPassword $destPassword `
            -dependencies $dependencies
    }
}

function Escrow-FoundationaLLM-Dependencies {
    param()

    $json = Get-Content -Path "./config/escrow-config.json" -Raw | ConvertFrom-Json
    $dependencies = $json.dependencies

    $destRegistry = $(azd env get-value FOUNDATIONALLM_ESCROW_REGISTRY)
    $destUsername = $(azd env get-value FOUNDATIONALLM_ESCROW_USERNAME)
    $destPassword = $(azd env get-value FOUNDATIONALLM_ESCROW_PASSWORD)

    foreach ($dependency in $dependencies) {
        Write-Host "Escrowing dependency $($dependency.name)..." -ForegroundColor Green
        $subDependencies = $dependency.dependencies
        Escrow-Helm-Chart `
            -service $dependency.name `
            -version $dependency.version `
            -helmChart $dependency.chart `
            -destRegistry $destRegistry `
            -destUsername $destUsername `
            -destPassword $destPassword `
            -dependencies $subDependencies

        azd env set INGRESS_ESCROWED $true
    }
}