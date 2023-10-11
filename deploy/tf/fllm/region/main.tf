locals {
  location           = var.location
  location_short     = var.location_short
  private_dns_zones  = var.private_dns_zones
  resource_groups    = var.resource_groups
  resource_prefix    = upper(join("-", [local.location_short, var.resource_prefix]))
  tags               = merge(var.tags, { workspace = terraform.workspace })
  vnet_address_space = var.vnet_address_space
}

data "azurerm_client_config" "current" {}

# module "logs" {
#   source          = "./modules/log-analytics-workspace"
#   resource_group  = local.resource_groups["OPS"]
#   resource_prefix = local.resource_prefix
#   tags            = local.tags
# }

# module "tfc_agent" {
#   source = "./modules/tfc-agent"

#   resource_prefix = "${local.resource_prefix}-tfca"
# }

module "ampls" {
  source          = "./modules/monitor-private-link-scope"
  resource_group  = azurerm_resource_group.rgs["OPS"]
  resource_prefix = local.resource_prefix
  tags            = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["Services"].id
    private_dns_zone_ids = [
      var.private_dns_zones["privatelink.blob.core.windows.net"].id,
      var.private_dns_zones["privatelink.monitor.azure.com"].id,
    ]
  }
}