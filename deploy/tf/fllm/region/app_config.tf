resource "azurerm_app_configuration" "app_config" {
  name                       = join("-", [local.resource_prefix, "appconfig"])
  location                   = local.location
  resource_group_name        = azurerm_resource_group.rgs["OPS"].name
  sku                        = "standard"
  public_network_access      = "Disabled"
  purge_protection_enabled   = false
  soft_delete_retention_days = 1

  identity {
    type = "SystemAssigned"
  }

  encryption {
    key_vault_key_identifier = azurerm_key_vault.ops_keyvault.id
  }

  tags = local.tags
}