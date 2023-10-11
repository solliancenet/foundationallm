module "regions" {
  source   = "./region"
  for_each = local.regions

  location           = each.key
  location_short     = each.value.location_short
  private_dns_zones  = module.global.private_dns_zones
  resource_groups    = local.regional_resource_groups
  resource_prefix    = local.resource_prefix
  tags               = local.tags
  tfc_agent_token    = var.tfc_agent_token
  vnet_address_space = each.value.vnet_address_space
}