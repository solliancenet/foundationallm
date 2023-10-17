resource "azurerm_network_profile" "linux_network_profile" {
  name                = "linuxnetprofile"
  location            = var.location
  resource_group_name = azurerm_resource_group.rgs["OPS"].name

  container_network_interface {
    name = "linuxnic"

    ip_configuration {
      name      = "linuxip"
      subnet_id = azurerm_subnet.subnets["Agents"].id
    }
  }
}