resource "azurerm_network_interface" "jumpbox" {
  location            = local.location
  name                = join("-", [local.resource_prefix, "JBX", "nic"])
  resource_group_name = azurerm_resource_group.rgs["JBX"].name

  ip_configuration {
    name                          = "internal"
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id          = azurerm_public_ip.jumpbox_ip.id
    subnet_id                     = azurerm_subnet.subnets["Jumpbox"].id
  }
}

resource "azurerm_public_ip" "jumpbox_ip" {
  allocation_method   = "Static"
  domain_name_label   = "fllm-jbx"
  location            = local.location
  name                = join("-", [local.resource_prefix, "JBX", "pip"])
  resource_group_name = azurerm_resource_group.rgs["NET"].name
  sku                 = "Standard"
}

resource "azurerm_windows_virtual_machine" "jumpbox" {
  admin_password        = "Test1234."
  admin_username        = "FLLMAdmin"
  location              = local.location
  name                  = lower(join("", concat(split("-", local.resource_prefix), ["vm"])))
  network_interface_ids = [azurerm_network_interface.jumpbox.id]
  resource_group_name   = azurerm_resource_group.rgs["JBX"].name
  size                  = "Standard_B2s"

  os_disk {
    caching              = "ReadWrite"
    storage_account_type = "Standard_LRS"
  }

  source_image_reference {
    offer     = "WindowsServer"
    publisher = "MicrosoftWindowsServer"
    sku       = "2016-Datacenter"
    version   = "latest"
  }
}