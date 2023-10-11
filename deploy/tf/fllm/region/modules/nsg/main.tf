resource "azurerm_network_security_group" "main" {
  location            = var.resource_group.location
  name                = join("-", [var.resource_prefix, "nsg"])
  resource_group_name = var.resource_group.name
  tags                = var.tags
}

resource "azurerm_subnet_network_security_group_association" "main" {
  network_security_group_id = azurerm_network_security_group.main.id
  subnet_id                 = var.subnet_id
}

resource "azurerm_network_security_rule" "inbound" {
  for_each = var.rules_inbound

  access                       = each.value.access
  destination_address_prefix   = each.value.destination_address_prefix
  destination_address_prefixes = each.value.destination_address_prefixes
  destination_port_range       = each.value.destination_port_range
  direction                    = "Inbound"
  name                         = each.key
  network_security_group_name  = azurerm_network_security_group.main.name
  priority                     = each.value.priority
  protocol                     = each.value.protocol
  resource_group_name          = var.resource_group.name
  source_address_prefix        = each.value.source_address_prefix
  source_address_prefixes      = each.value.source_address_prefixes
  source_port_range            = each.value.source_port_range
}

resource "azurerm_network_security_rule" "outbound" {
  for_each = var.rules_outbound

  access                       = each.value.access
  destination_address_prefix   = each.value.destination_address_prefix
  destination_address_prefixes = each.value.destination_address_prefixes
  destination_port_range       = each.value.destination_port_range
  direction                    = "Outbound"
  name                         = each.key
  network_security_group_name  = azurerm_network_security_group.main.name
  priority                     = each.value.priority
  protocol                     = each.value.protocol
  resource_group_name          = var.resource_group.name
  source_address_prefix        = each.value.source_address_prefix
  source_address_prefixes      = each.value.source_address_prefixes
  source_port_range            = each.value.source_port_range
}