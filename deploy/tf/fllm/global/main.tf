locals {
  location        = var.location
  location_short  = var.location_short
  resource_groups = var.resource_groups
  resource_prefix = upper(join("-", [local.location_short, var.resource_prefix]))
  tags            = var.tags

  private_dns_zones = {
    "azure-api.net"                           = {}
    "developer.azure-api.net"                 = {}
    "management.azure-api.net"                = {}
    "portal.azure-api.net"                    = {}
    "privatelink.azconfig.io"                 = {}
    "privatelink.azure-api.net"               = {}
    "privatelink.azurewebsites.net"           = {}
    "privatelink.blob.core.windows.net"       = {}
    "privatelink.cognitiveservices.azure.com" = {}
    "privatelink.database.windows.net"        = {}
    "privatelink.dfs.core.windows.net"        = {}
    "privatelink.documents.azure.com"         = {}
    "privatelink.file.core.windows.net"       = {}
    "privatelink.monitor.azure.com"           = {}
    "privatelink.openai.azure.com"            = {}
    "privatelink.queue.core.windows.net"      = {}
    "privatelink.search.windows.net"          = {}
    "privatelink.table.core.windows.net"      = {}
    "privatelink.vaultcore.azure.net"         = {}
    "scm.azure-api.net"                       = {}
    "privatelink.eastus.azmk8s.io"            = {}
  }
}