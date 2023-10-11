locals {
  default_nsg_rules = {
    inbound = {
      # "deny-all-inbound" = {
      #   access                     = "Deny"
      #   destination_address_prefix = "*"
      #   destination_port_range     = "*"
      #   priority                   = 4096
      #   protocol                   = "*"
      #   source_address_prefix      = "*"
      #   source_port_range          = "*"
      # }
    }
    outbound = {
      # "deny-all-outbound" = {
      #   access                     = "Deny"
      #   destination_address_prefix = "*"
      #   destination_port_range     = "*"
      #   priority                   = 4096
      #   protocol                   = "*"
      #   source_address_prefix      = "*"
      #   source_port_range          = "*"
      # }
    }
  }

  subnets = {
    "AppGateway" = {
      address_prefix    = cidrsubnet(local.vnet_address_space, 8, 0)
      service_endpoints = []

      nsg_rules = {
        inbound  = merge(local.default_nsg_rules.inbound, {})
        outbound = merge(local.default_nsg_rules.outbound, {})
      }
    }
    "Services" = {
      address_prefix    = cidrsubnet(local.vnet_address_space, 8, 1)
      service_endpoints = []

      nsg_rules = {
        inbound  = merge(local.default_nsg_rules.inbound, {})
        outbound = merge(local.default_nsg_rules.outbound, {})
      }
    }
    "Datasources" = {
      address_prefix    = cidrsubnet(local.vnet_address_space, 8, 2)
      service_endpoints = []

      nsg_rules = {
        inbound  = merge(local.default_nsg_rules.inbound, {})
        outbound = merge(local.default_nsg_rules.outbound, {})
      }
    }
    "FLLMServices" = {
      address_prefix    = cidrsubnet(local.vnet_address_space, 8, 3)
      service_endpoints = []

      nsg_rules = {
        inbound  = merge(local.default_nsg_rules.inbound, {})
        outbound = merge(local.default_nsg_rules.outbound, {})
      }
    }
    "FLLMStorage" = {
      address_prefix    = cidrsubnet(local.vnet_address_space, 8, 4)
      service_endpoints = []

      nsg_rules = {
        inbound  = merge(local.default_nsg_rules.inbound, {})
        outbound = merge(local.default_nsg_rules.outbound, {})
      }
    }
    "FLLMOpenAI" = {
      address_prefix = cidrsubnet(local.vnet_address_space, 8, 5)
      service_endpoints = [
        "Microsoft.CognitiveServices"
      ]

      nsg_rules = {
        inbound = merge(local.default_nsg_rules.inbound, {
          "allow-apim" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "3443"
            priority                   = 128
            protocol                   = "Tcp"
            source_address_prefix      = "ApiManagement"
            source_port_range          = "*"
          }
          "allow-lb" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "6390"
            priority                   = 192
            protocol                   = "Tcp"
            source_address_prefix      = "AzureLoadBalancer"
            source_port_range          = "*"
          }
        })
        outbound = merge(local.default_nsg_rules.outbound, {
          "allow-storage" = {
            access                     = "Allow"
            destination_address_prefix = "Storage"
            destination_port_range     = "443"
            priority                   = 128
            protocol                   = "Tcp"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
          "allow-sql" = {
            access                     = "Allow"
            destination_address_prefix = "SQL"
            destination_port_range     = "1443"
            priority                   = 192
            protocol                   = "Tcp"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
          "allow-kv" = {
            access                     = "Allow"
            destination_address_prefix = "AzureKeyVault"
            destination_port_range     = "443"
            priority                   = 224
            protocol                   = "Tcp"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
          "allow-vnet" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 4068
            protocol                   = "*"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
        })
      }
    }
    "Vectorization" = {
      address_prefix    = cidrsubnet(local.vnet_address_space, 8, 6)
      service_endpoints = []

      nsg_rules = {
        inbound  = merge(local.default_nsg_rules.inbound, {})
        outbound = merge(local.default_nsg_rules.outbound, {})
      }
    }
    "Jumpbox" = {
      address_prefix    = cidrsubnet(local.vnet_address_space, 8, 7)
      service_endpoints = []

      nsg_rules = {
        inbound = merge(local.default_nsg_rules.inbound, {
          "allow-rdp" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "3389"
            priority                   = 128
            protocol                   = "Tcp"
            source_address_prefix      = "Internet"
            source_port_range          = "*"
          }
          "allow-vnet" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 192
            protocol                   = "*"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
        })

        outbound = merge(local.default_nsg_rules.outbound, {
          "allow-vnet" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 128
            protocol                   = "*"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
        })
      }
    }
    "Gateway" = {
      address_prefix    = cidrsubnet(local.vnet_address_space, 8, 8)
      service_endpoints = []

      nsg_rules = {
        inbound = merge(local.default_nsg_rules.inbound, {})

        outbound = merge(local.default_nsg_rules.outbound, {})
      }
    }
    # Small networks at the end
    "tfc" = {
      address_prefix    = cidrsubnet(local.vnet_address_space, 11, 2046)
      service_endpoints = []
      delegation = {
        "Microsoft.ContainerInstance/containerGroups" = [
          "Microsoft.Network/virtualNetworks/subnets/action"
        ]
      }

      nsg_rules = {
        inbound = merge(local.default_nsg_rules.inbound, {})
        outbound = merge(local.default_nsg_rules.outbound, {
          "allow-tfc-api" = {
            access                       = "Allow"
            destination_address_prefixes = data.tfe_ip_ranges.tfc.api
            destination_port_range       = "443"
            priority                     = 128
            protocol                     = "Tcp"
            source_address_prefix        = "*"
            source_port_range            = "*"
          }
          "allow-tfc-notifications" = {
            access                       = "Allow"
            destination_address_prefixes = data.tfe_ip_ranges.tfc.notifications
            destination_port_range       = "443"
            priority                     = 160
            protocol                     = "Tcp"
            source_address_prefix        = "*"
            source_port_range            = "*"
          }
          "allow-tfc-sentinel" = {
            access                       = "Allow"
            destination_address_prefixes = data.tfe_ip_ranges.tfc.sentinel
            destination_port_range       = "443"
            priority                     = 192
            protocol                     = "Tcp"
            source_address_prefix        = "*"
            source_port_range            = "*"
          }
          "allow-tfc-vcs" = {
            access                       = "Allow"
            destination_address_prefixes = data.tfe_ip_ranges.tfc.vcs
            destination_port_range       = "443"
            priority                     = 224
            protocol                     = "Tcp"
            source_address_prefix        = "*"
            source_port_range            = "*"
          }
          "allow-tfc-services" = {
            access                     = "Allow"
            destination_address_prefix = "Internet"
            destination_port_range     = "443"
            priority                   = 256
            protocol                   = "Tcp"
            source_address_prefix      = "*"
            source_port_range          = "*"
          }
          "allow-vnet" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 4068
            protocol                   = "*"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
        })
      }
    }
  }
}

data "tfe_ip_ranges" "tfc" {}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_vnet_link" {
  for_each = local.private_dns_zones

  name                  = each.key
  private_dns_zone_name = each.key
  resource_group_name   = each.value.resource_group_name
  tags                  = local.tags
  virtual_network_id    = azurerm_virtual_network.vnet.id
}

resource "azurerm_subnet" "subnets" {
  for_each = local.subnets

  address_prefixes     = [each.value.address_prefix]
  name                 = each.key
  resource_group_name  = azurerm_resource_group.rgs["NET"].name
  service_endpoints    = each.value.service_endpoints
  virtual_network_name = azurerm_virtual_network.vnet.name

  dynamic "delegation" {
    for_each = lookup(each.value, "delegation", {})
    content {
      name = "${delegation.key}-delegation"

      service_delegation {
        actions = delegation.value
        name    = delegation.key
      }
    }
  }
}

resource "azurerm_virtual_network" "vnet" {
  address_space       = [local.vnet_address_space]
  location            = local.location
  name                = join("-", [local.resource_prefix, "vnet"])
  resource_group_name = azurerm_resource_group.rgs["NET"].name
  tags                = local.tags
}

module "nsg_tfc" {
  for_each = local.subnets
  source   = "./modules/nsg"

  resource_group  = azurerm_resource_group.rgs["NET"]
  resource_prefix = "${local.resource_prefix}-${each.key}"
  rules_inbound   = each.value.nsg_rules.inbound
  rules_outbound  = each.value.nsg_rules.outbound
  subnet_id       = azurerm_subnet.subnets[each.key].id
  tags            = local.tags
}