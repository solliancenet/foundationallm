output "certificate_secret_id" {
  description = "The ID of the certificate secret in the key vault."
  value       = azurerm_key_vault_certificate.certificate.versionless_secret_id
}

output "pem_secret_ids" {
  description = "The IDs of the PEM secrets in the key vault."
  value = {
    ca   = azurerm_key_vault_secret.ca.resource_versionless_id
    cert = azurerm_key_vault_secret.certificate.resource_versionless_id
    key  = azurerm_key_vault_secret.key.resource_versionless_id
  }
}