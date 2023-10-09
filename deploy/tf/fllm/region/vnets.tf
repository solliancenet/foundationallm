resource "azurerm_virtual_network" "vnet" {
  address_space       = [local.vnet_address_space]
  location            = local.location
  name                = join("-", [local.resource_prefix, "vnet"])
  resource_group_name = azurerm_resource_group.rgs["NET"].name
}

locals {
  subnets = {
    "AppGateway" = { address_prefix = cidrsubnet(local.vnet_address_space, 8, 0) }
    "Services" = { address_prefix = cidrsubnet(local.vnet_address_space, 8, 1) }
    "Datasources" = { address_prefix = cidrsubnet(local.vnet_address_space, 8, 2) }
    "FLLMServices" = { address_prefix = cidrsubnet(local.vnet_address_space, 8, 3) }
    "FLLMStorage" = { address_prefix = cidrsubnet(local.vnet_address_space, 8, 4) }
    "FLLMOpenAI" = { address_prefix = cidrsubnet(local.vnet_address_space, 8, 5) }
    "Vectorization" = { address_prefix = cidrsubnet(local.vnet_address_space, 8, 6)}
  }
}

resource "azurerm_subnet" "subnets" {
  for_each = local.subnets

  address_prefixes     = [each.value.address_prefix]
  name                 = each.key
  resource_group_name  = azurerm_resource_group.rgs["NET"].name
  virtual_network_name = azurerm_virtual_network.vnet.name
}