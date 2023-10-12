resource "azurerm_search_service" "cog_search" {
  location                      = local.location
  name                          = lower(join("", concat(split("-", local.resource_prefix), ["VEC", "cogsvc"])))
  resource_group_name           = azurerm_resource_group.rgs["VEC"].name
  sku                           = "standard"
  public_network_access_enabled = false
}

resource "azurerm_private_endpoint" "cog_search_ple" {
  location            = local.location
  name                = join("-", [local.resource_prefix, "VEC", "cogsvc", "ple"])
  resource_group_name = azurerm_resource_group.rgs["NET"].name
  subnet_id           = azurerm_subnet.subnets["Vectorization"].id

  private_service_connection {
    is_manual_connection           = false
    name                           = join("-", [local.resource_prefix, "VEC", "cogsvc", "psc"])
    private_connection_resource_id = azurerm_search_service.cog_search.id
    subresource_names              = ["searchService"]
  }

  private_dns_zone_group {
    name                 = join("-", [local.resource_prefix, "VEC", "cogsvc", "dzg"])
    private_dns_zone_ids = [local.private_dns_zones["privatelink.search.windows.net"].id]
  }
}

