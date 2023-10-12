resource "azurerm_cognitive_account" "cog_search" {

  kind                          = "CognitiveServices"
  location                      = local.location
  name                          = join("-", [local.resource_prefix, "VEC", "cogsvc"])
  resource_group_name           = azurerm_resource_group.rgs["VEC"].name
  sku_name                      = "S0"
  custom_subdomain_name         = lower(join("-", [local.resource_prefix, "VEC"]))
  public_network_access_enabled = false
}

resource "azurerm_private_endpoint" "cog_search_ple" {
  location            = local.location
  name                = join("-", [local.resource_prefix, "VEC", "cogsvc", "ple"])
  resource_group_name = azurerm_resource_group.rgs["VEC"].name
  subnet_id           = azurerm_subnet.subnets["Vectorization"].id

  private_service_connection {
    is_manual_connection           = false
    name                           = join("-", [local.resource_prefix, "VEC", "cogsvc", "psc"])
    private_connection_resource_id = azurerm_cognitive_account.cog_search.id
    subresource_names              = ["account"]
  }

  private_dns_zone_group {
    name                 = join("-", [local.resource_prefix, "VEC", "cogsvc", "dzg"])
    private_dns_zone_ids = [local.private_dns_zones["privatelink.cognitiveservices.azure.com"].id]
  }
}

