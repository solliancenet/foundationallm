module "global" {
  source = "./global"

  location        = local.global_location
  resource_groups = local.global_resource_groups
  resource_prefix = local.resource_prefix
  tags            = local.tags
}
