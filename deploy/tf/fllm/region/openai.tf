locals {
  openai = {
    OAI1 = {}
    OAI2 = {}
    OAI3 = {}
    OAI4 = {}
  }
}

resource "azurerm_cognitive_account" "openai" {
  for_each = local.openai

  name                          = join("-", [local.resource_prefix, each.key, "openai"])
  location                      = local.location
  resource_group_name           = azurerm_resource_group.rgs["OAI"].name
  kind                          = "OpenAI"
  custom_subdomain_name         = lower(join("-", [local.resource_prefix, each.key]))
  public_network_access_enabled = false

  sku_name = "S0"

  tags = local.tags
}

resource "azurerm_private_endpoint" "openai_ple" {
  for_each = azurerm_cognitive_account.openai

  name                = join("-", [local.resource_prefix, each.key, "openai", "ple"])
  location            = local.location
  resource_group_name = azurerm_resource_group.rgs["NET"].name
  subnet_id           = azurerm_subnet.subnets["FLLMOpenAI"].id

  private_service_connection {
    is_manual_connection           = false
    name                           = join("-", [local.resource_prefix, each.key, "openai", "psc"])
    private_connection_resource_id = each.value.id
    subresource_names              = ["account"]
  }

  private_dns_zone_group {
    name                 = join("-", [local.resource_prefix, each.key, "openai", "dzg"])
    private_dns_zone_ids = [local.private_dns_zones["privatelink.openai.azure.com"].id]
  }
}