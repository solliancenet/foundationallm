locals {
  location        = var.location
  location_short  = var.location_short
  resource_groups = var.resource_groups
  resource_prefix = upper(join("-", [local.location_short, var.resource_prefix]))
  tags            = var.tags

  private_dns_zones = {
    "azure-api.net"                     = {}
    "developer.azure-api.net"           = {}
    "management.azure-api.net"          = {}
    "portal.azure-api.net"              = {}
    "privatelink.azure-api.net"         = {}
    "privatelink.blob.core.windows.net" = {}
    "privatelink.monitor.azure.com"     = {}
    "privatelink.openai.azure.com"      = {}
    "privatelink.vaultcore.azure.com"   = {}
    "scm.azure-api.net"                 = {}
  }
}