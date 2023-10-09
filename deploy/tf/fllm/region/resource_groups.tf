resource "azurerm_resource_group" "rgs" {
  for_each = local.resource_groups

  location = local.location
  name     = join("-", [local.resource_prefix, each.key, "rg"])
}
