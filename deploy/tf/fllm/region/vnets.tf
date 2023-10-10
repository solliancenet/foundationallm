locals {
  subnets = {
    "AppGateway"    = {
      address_prefix = cidrsubnet(local.vnet_address_space, 8, 0)
      service_endpoints = []
    }
    "Services"      = {
      address_prefix = cidrsubnet(local.vnet_address_space, 8, 1)
      service_endpoints = []
    }
    "Datasources"   = {
      address_prefix = cidrsubnet(local.vnet_address_space, 8, 2)
      service_endpoints = []
    }
    "FLLMServices"  = {
      address_prefix = cidrsubnet(local.vnet_address_space, 8, 3)
      service_endpoints = []
    }
    "FLLMStorage"   = {
      address_prefix = cidrsubnet(local.vnet_address_space, 8, 4)
      service_endpoints = []
    }
    "FLLMOpenAI"    = {
      address_prefix = cidrsubnet(local.vnet_address_space, 8, 5)
      service_endpoints = [
        "Microsoft.CognitiveServices"
      ]
    }
    "Vectorization" = {
      address_prefix = cidrsubnet(local.vnet_address_space, 8, 6)
      service_endpoints = []
    }
    "Jumpbox"       = {
      address_prefix = cidrsubnet(local.vnet_address_space, 8, 7)
      service_endpoints = []
    }
    "Gateway"       = {
      address_prefix = cidrsubnet(local.vnet_address_space, 8, 8)
      service_endpoints = []
    }
  }
}

resource "azurerm_virtual_network" "vnet" {
  address_space       = [local.vnet_address_space]
  location            = local.location
  name                = join("-", [local.resource_prefix, "vnet"])
  resource_group_name = azurerm_resource_group.rgs["NET"].name

  tags = local.tags
}

resource "azurerm_subnet" "subnets" {
  for_each = local.subnets

  address_prefixes     = [each.value.address_prefix]
  name                 = each.key
  resource_group_name  = azurerm_resource_group.rgs["NET"].name
  virtual_network_name = azurerm_virtual_network.vnet.name
  service_endpoints = each.value.service_endpoints
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_vnet_link" {
  for_each = local.private_dns_zones

  name                  = each.key
  private_dns_zone_name = each.key
  resource_group_name   = each.value.resource_group_name
  virtual_network_id    = azurerm_virtual_network.vnet.id

  tags = local.tags
}

resource "azurerm_network_security_group" "openai_nsg" {
  name                = join("-", [local.resource_prefix, "OAI", "nsg"])
  location            = local.location
  resource_group_name = azurerm_resource_group.rgs["NET"].name

  tags = local.tags
}

resource "azurerm_network_security_rule" "openai_nsr_1" {
  access                      = "Allow"
  direction                   = "Inbound"
  name                        = "management"
  network_security_group_name = azurerm_network_security_group.openai_nsg.name
  priority                    = 100
  protocol                    = "Tcp"
  resource_group_name         = azurerm_resource_group.rgs["NET"].name
  source_port_range           = "*"
  destination_port_range      = "3443"
  source_address_prefix       = "ApiManagement"
  destination_address_prefix  = "VirtualNetwork"
}

resource "azurerm_network_security_rule" "openai_nsr_2" {
  access                      = "Allow"
  direction                   = "Inbound"
  name                        = "loadbalancing"
  network_security_group_name = azurerm_network_security_group.openai_nsg.name
  priority                    = 101
  protocol                    = "Tcp"
  resource_group_name         = azurerm_resource_group.rgs["NET"].name
  source_port_range           = "*"
  destination_port_range      = "6390"
  source_address_prefix       = "AzureLoadBalancer"
  destination_address_prefix  = "VirtualNetwork"
}

resource "azurerm_network_security_rule" "openai_nsr_3" {
  access                      = "Allow"
  direction                   = "Outbound"
  name                        = "storage"
  network_security_group_name = azurerm_network_security_group.openai_nsg.name
  priority                    = 102
  protocol                    = "Tcp"
  resource_group_name         = azurerm_resource_group.rgs["NET"].name
  source_port_range           = "*"
  destination_port_range      = "443"
  source_address_prefix       = "VirtualNetwork"
  destination_address_prefix  = "Storage"
}

resource "azurerm_network_security_rule" "openai_nsr_4" {
  access                      = "Allow"
  direction                   = "Outbound"
  name                        = "sql"
  network_security_group_name = azurerm_network_security_group.openai_nsg.name
  priority                    = 103
  protocol                    = "Tcp"
  resource_group_name         = azurerm_resource_group.rgs["NET"].name
  source_port_range           = "*"
  destination_port_range      = "1443"
  source_address_prefix       = "VirtualNetwork"
  destination_address_prefix  = "SQL"
}

resource "azurerm_network_security_rule" "openai_nsr_5" {
  access                      = "Allow"
  direction                   = "Outbound"
  name                        = "keyvault"
  network_security_group_name = azurerm_network_security_group.openai_nsg.name
  priority                    = 104
  protocol                    = "Tcp"
  resource_group_name         = azurerm_resource_group.rgs["NET"].name
  source_port_range           = "*"
  destination_port_range      = "443"
  source_address_prefix       = "VirtualNetwork"
  destination_address_prefix  = "AzureKeyVault"
}

resource "azurerm_subnet_network_security_group_association" "openai_nsg" {
  subnet_id                 = azurerm_subnet.subnets["FLLMOpenAI"].id
  network_security_group_id = azurerm_network_security_group.openai_nsg.id
}

resource "azurerm_network_security_group" "jbx_nsg" {
  name                = join("-", [local.resource_prefix, "JBX", "nsg"])
  location            = local.location
  resource_group_name = azurerm_resource_group.rgs["NET"].name

  tags = local.tags
}

resource "azurerm_network_security_rule" "jbx_nsr_1" {
  access                      = "Allow"
  direction                   = "Inbound"
  name                        = "rdp"
  network_security_group_name = azurerm_network_security_group.jbx_nsg.name
  priority                    = 100
  protocol                    = "Tcp"
  resource_group_name         = azurerm_resource_group.rgs["NET"].name
  source_port_range           = "*"
  destination_port_range      = "3389"
  source_address_prefix       = "Internet"
  destination_address_prefix  = "VirtualNetwork"
}

resource "azurerm_subnet_network_security_group_association" "jbx_nsg" {
  subnet_id                 = azurerm_subnet.subnets["Jumpbox"].id
  network_security_group_id = azurerm_network_security_group.jbx_nsg.id
}

