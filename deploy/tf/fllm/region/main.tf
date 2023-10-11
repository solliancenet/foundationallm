locals {
  location           = var.location
  location_short     = var.location_short
  private_dns_zones  = var.private_dns_zones
  resource_groups    = var.resource_groups
  resource_prefix    = upper(join("-", [local.location_short, var.resource_prefix]))
  tags               = var.tags
  vnet_address_space = var.vnet_address_space
}

data "azurerm_client_config" "current" {}