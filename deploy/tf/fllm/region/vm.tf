resource "azurerm_public_ip" "jumpbox_ip" {
  allocation_method   = "Static"
  location            = local.location
  name                = join("-", [local.resource_prefix, "JBX", "pip"])
  resource_group_name = azurerm_resource_group.rgs["NET"].name
  sku                 = "Standard"
  domain_name_label   = "fllm-jbx"
}

resource "azurerm_network_interface" "jumpbox" {
  name                = join("-", [local.resource_prefix, "JBX", "nic"])
  location            = local.location
  resource_group_name = azurerm_resource_group.rgs["JBX"].name

  ip_configuration {
    name                          = "internal"
    private_ip_address_allocation = "Dynamic"
    subnet_id                     = azurerm_subnet.subnets["Jumpbox"].id
    public_ip_address_id          = azurerm_public_ip.jumpbox_ip.id
  }
}

resource "azurerm_windows_virtual_machine" "jumpbox" {
  name                  = lower(join("", concat(split("-", local.resource_prefix), ["vm"])))
  resource_group_name   = azurerm_resource_group.rgs["JBX"].name
  location              = local.location
  size                  = "Standard_B2s"
  admin_username        = "FLLMAdmin"
  admin_password        = "Test1234."
  network_interface_ids = [azurerm_network_interface.jumpbox.id]

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