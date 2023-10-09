locals {
  location        = var.location
  location_short  = var.location_short
  resource_groups = var.resource_groups
  resource_prefix = upper(join("-", [local.location_short, var.resource_prefix]))
  tags            = var.tags
  vnet_address_space = var.vnet_address_space
}