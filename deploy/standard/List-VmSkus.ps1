az vm list-skus --location eastus --query "[?resourceType=='virtualMachines']" --output table

# Define the path to the JSON file
$jsonFilePath = "C:\foundationallm\skus.json"

# Import the JSON data
$skus = Get-Content -Path $jsonFilePath | ConvertFrom-Json

# Filter the data to show virtual machines with no restrictions
$filteredSkus = $skus | Where-Object { $_.resourceType -eq "virtualMachines" -and $_.restrictions.Count -eq 0 }

# Display the filtered data
$filteredSkus | Format-Table name, resourceType, restrictions, size, tier -AutoSize

# Optionally, export the filtered data to a TSV file
$outputFile = "C:\foundationallm\filtered_skus.tsv"
$filteredSkus | Export-Csv -Delimiter "`t" -Path $outputFile -NoTypeInformation

Write-Output "Filtered SKUs have been exported to $outputFile"


#####

# Define the path to the JSON file
$jsonFilePath = "C:\foundationallm\skus.json"

# Import the JSON data
$skus = Get-Content -Path $jsonFilePath | ConvertFrom-Json -AsHashtable

# Debugging output to verify the imported data
Write-Output "Imported SKUs:"
$skus | Format-Table name, resourceType, restrictions, size, tier -AutoSize

# Filter the data to show virtual machines with no restrictions
$filteredSkus = $skus | Where-Object { $_.resourceType -eq "virtualMachines" -and $_.restrictions.Count -eq 0 }

# Debugging output to verify the filtered data
Write-Output "Filtered SKUs:"
$filteredSkus | Format-Table name, resourceType, restrictions, size, tier -AutoSize

# Export the filtered data to a TSV file
$outputFile = "C:\foundationallm\filtered_skus.tsv"
$filteredSkus | Export-Csv -Delimiter "`t" -Path $outputFile -NoTypeInformation

Write-Output "Filtered SKUs have been exported to $outputFile"