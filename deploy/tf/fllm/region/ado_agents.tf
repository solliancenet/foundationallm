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

resource "azurerm_container_group" "linux-container-group" {
  name                = "${local.resource_prefix}-ado-aci"
  location            = azurerm_resource_group.rgs["OPS"].location
  resource_group_name = azurerm_resource_group.rgs["OPS"].name
  ip_address_type     = "Private"
  os_type             = "Linux"
  subnet_ids          = [azurerm_subnet.subnets["Agents"].id]

  container {
    name   = "${local.resource_prefix}-ado-agent"
    image  = "kgopi1/azselfhostedagent:latest"
    cpu    = 1
    memory = 1.5

    # this field seems to be mandatory (error happens if not there). See https://github.com/terraform-providers/terraform-provider-azurerm/issues/1697#issuecomment-608669422
    ports {
      port     = 8080
      protocol = "TCP"
    }

    environment_variables = {
      AZP_URL        = azurerm_key_vault_secret.ado_secrets["AZP-URL"].value
      AZP_TOKEN      = azurerm_key_vault_secret.ado_secrets["AZP-TOKEN"].value
      AZP_AGENT_NAME = azurerm_key_vault_secret.ado_secrets["AZP-AGENT-NAME"].value
    }
  }

  restart_policy = "OnFailure"

  # identity block generated depending on cases
  # if a system assigned managed identity only is requested
  identity {
    type = "SystemAssigned"
  }
}