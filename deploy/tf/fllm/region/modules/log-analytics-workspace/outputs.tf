output "data_collection_rule_id" {
  description = "The Data Collection Rule ID."
  value       = azurerm_monitor_data_collection_rule.dcr.id
}

output "location" {
  description = "The location of the Log Analytics Workspace."
  value       = azurerm_log_analytics_workspace.workspace.location
}

output "id" {
  description = "The Log Analytics Workspace Resource ID."
  value       = azurerm_log_analytics_workspace.workspace.id
}

output "primary_shared_key" {
  description = "The primary access key."
  sensitive   = true
  value       = azurerm_log_analytics_workspace.workspace.primary_shared_key
}

output "workspace_id" {
  description = "The Log Analytics Workspace ID."
  value       = azurerm_log_analytics_workspace.workspace.workspace_id
}
