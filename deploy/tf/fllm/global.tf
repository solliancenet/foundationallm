module "global" {
  source = "./global"

  location        = local.global_location
  public_domain   = local.public_domain
  resource_groups = local.global_resource_groups
  resource_prefix = local.resource_prefix
  tags            = local.tags
}
