output "private_dns_zones" {
  description = "The the private DNS zones."
  value       = azurerm_private_dns_zone.private_dns
}

output "public_dns_zone" {
  description = "The public DNS zone."
  value       = data.azurerm_dns_zone.public_dns
}