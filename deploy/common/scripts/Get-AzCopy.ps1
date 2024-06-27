#! /usr/bin/pwsh
<#
.SYNOPSIS
    Downloads and sets up AzCopy tool based on the operating system.

.DESCRIPTION
    This script checks the operating system and downloads the corresponding version of AzCopy. 
	It handles the setup for Windows, macOS, and Linux. 
	It also checks if AzCopy is already installed and verifies the login status.

.PARAMETER AZCOPY_VERSION
    Specifies the version of AzCopy to download and use. This parameter is fixed in the script.

.EXAMPLE
    .\Get-AzCopy.ps1

.NOTES
    Ensure that the PowerShell session has sufficient permissions to download and execute files, and that internet connectivity is available.
#>

# Disable command echo, Set strict mode, and stop script execution on any errors
Set-PSDebug -Trace 0
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# AzCopy tool version to be downloaded
$AZCOPY_VERSION = "10.25.0"
$env:AZCOPY_AUTO_LOGIN_TYPE = "AZCLI"

# Determine OS and set the download URL and file extension accordingly
if ($IsWindows) {
	$url = "https://azcopyvnext.azureedge.net/releases/release-10.25.0-20240522/azcopy_windows_amd64_10.25.0.zip"
	$os = "windows"
	$ext = "zip"
}elseif ($IsMacOS) {
	$url = "https://azcopyvnext.azureedge.net/releases/release-10.25.0-20240522/azcopy_darwin_amd64_10.25.0.zip"
	$os = "darwin"
	$ext = "zip"
}elseif ($IsLinux) {
	$url = "https://azcopyvnext.azureedge.net/releases/release-10.25.0-20240522/azcopy_linux_amd64_10.25.0.tar.gz"
	$os = "linux"
	$ext = "tar.gz"
}

# Define paths for download and extraction
$archivePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("../tools/azcopy.${ext}")
$destinationPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("../tools")
$extractedPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("../tools/azcopy_${os}_amd64_${AZCOPY_VERSION}")
$renamedPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("../tools/azcopy")
$toolPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("../tools/azcopy/azcopy")

# Check if AzCopy already exists, download and extract if not
if (Test-Path -Path "../tools/azcopy") {
	Write-Host "azcopy already exists."
}
else {
	Invoke-WebRequest -Uri $url -OutFile $archivePath
	
	if ($IsLinux) {
		tar -xvzf $archivePath -C $destinationPath
	}
	else {
		Expand-Archive -Path $archivePath -DestinationPath $destinationPath
	}

	Move-Item -Path $extractedPath -Destination $renamedPath

	if (!$IsWindows) {
		chmod +x $toolPath
	}
}

Exit 0
