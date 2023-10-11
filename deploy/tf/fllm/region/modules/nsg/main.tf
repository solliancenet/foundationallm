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

