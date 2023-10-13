output "id" {
  description = "The App Configuration service Resource ID."
  value       = azurerm_app_configuration.main.id
}

output "app_config_mi" {
  description = "The App Configuration managed identity."
  value       = azurerm_user_assigned_identity.app_config_mi
}