output "id" {
  description = "Storage Account ID."
  value       = azurerm_storage_account.main.id
}

output "name" {
  description = "The storage account name."
  value       = azurerm_storage_account.main.name
}

output "primary_access_key" {
  description = "The storage account primary access key."
  sensitive   = true
  value       = azurerm_storage_account.main.primary_access_key
}

output "primary_blob_endpoint" {
  description = "The storage account primary blob endpoint."
  value       = azurerm_storage_account.main.primary_blob_endpoint
}

output "primary_connection_string" {
  description = "The storage account primary connection string."
  sensitive   = true
  value       = azurerm_storage_account.main.primary_connection_string
}