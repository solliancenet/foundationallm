locals {
  location        = var.location
  location_short  = var.location_short
  resource_groups = var.resource_groups
  resource_prefix = upper(join("-", [local.location_short, var.resource_prefix]))
  tags            = var.tags

  private_dns_zones = {
    "privatelink.openai.azure.com"    = {}
    "privatelink.vaultcore.azure.com" = {}
    "privatelink.azure-api.net"       = {}
    "azure-api.net"                   = {}
    "portal.azure-api.net"            = {}
    "developer.azure-api.net"         = {}
    "management.azure-api.net"        = {}
    "scm.azure-api.net"               = {}
  }
}