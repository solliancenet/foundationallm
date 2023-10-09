resource "azurerm_private_dns_zone" "private_dns" {
  for_each = local.private_dns_zones

  name = each.key
  resource_group_name = azurerm_resource_group.rgs["DNS"].name
}