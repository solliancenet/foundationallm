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
$AZCOPY_VERSION = "10.28.0"

# Determine OS and set the download URL and file extension accordingly
if ($IsWindows) {
	$url = "https://azcopyvnext-awgzd8g7aagqhzhe.b02.azurefd.net/releases/release-10.28.0-20250127/azcopy_windows_amd64_10.28.0.zip"
	$os = "windows"
	$ext = "zip"
}elseif ($IsMacOS) {
	$url = "https://azcopyvnext-awgzd8g7aagqhzhe.b02.azurefd.net/releases/release-10.28.0-20250127/azcopy_darwin_amd64_10.28.0.zip"
	$os = "darwin"
	$ext = "zip"
}elseif ($IsLinux) {
	$url = "https://azcopyvnext-awgzd8g7aagqhzhe.b02.azurefd.net/releases/release-10.28.0-20250127/azcopy_linux_amd64_10.28.0.tar.gz"
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
	Write-Host -ForegroundColor Blue "AzCopy already exists."
}
else {
	Write-Host -ForegroundColor Yellow "Downloading AzCopy version $AZCOPY_VERSION for $os..."
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
Write-Host -ForegroundColor Green "AzCopy setup completed successfully."

Exit 0
