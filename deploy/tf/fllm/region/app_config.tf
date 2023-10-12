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

  # TODO - Consider using a user assigned MI to mitigate the
  # resource contention that occurs when provisioning the App Configuration
  # service with Azure KeyVault encryption
#  encryption {
#    key_vault_key_identifier = azurerm_key_vault_key.app_config_key.id
#  }

  tags = local.tags
}

resource "azurerm_key_vault_key" "app_config_key" {
  name         = join("-", [local.resource_prefix, "appconfig", "key"])
  key_vault_id = azurerm_key_vault.ops_keyvault.id
  key_type     = "RSA"
  key_size     = 2048
  key_opts = [
    "decrypt",
    "encrypt",
    "sign",
    "unwrapKey",
    "verify",
    "wrapKey"
  ]
}

resource "azurerm_private_endpoint" "app_config_ple" {
  location            = local.location
  name                = join("-", [local.resource_prefix, "appconfig", "ple"])
  resource_group_name = azurerm_resource_group.rgs["NET"].name
  subnet_id           = azurerm_subnet.subnets["FLLMServices"].id

  private_service_connection {
    is_manual_connection           = false
    name                           = join("-", [local.resource_prefix, "appconfig", "psc"])
    private_connection_resource_id = azurerm_app_configuration.app_config.id
    subresource_names              = ["configurationStores"]
  }

  private_dns_zone_group {
    name                 = join("-", [local.resource_prefix, "appconfig", "dzg"])
    private_dns_zone_ids = [local.private_dns_zones["privatelink.azconfig.io"].id]
  }
}