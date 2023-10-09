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

  name                = join("-", [local.resource_prefix, each.key, "openai"])
  location            = local.location
  resource_group_name = azurerm_resource_group.rgs["OAI"].name
  kind                = "OpenAI"

  sku_name = "S0"

  tags = local.tags
}