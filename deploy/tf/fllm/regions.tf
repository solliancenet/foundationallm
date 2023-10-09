module "regions" {
  source   = "./region"
  for_each = local.regions

  location        = each.key
  location_short  = each.value.location_short
  resource_groups = local.regional_resource_groups
  resource_prefix = local.resource_prefix
  tags            = local.tags
  vnet_address_space = each.value.vnet_address_space
}