resource "azurerm_key_vault" "openai_keyvault" {
  enable_rbac_authorization     = true
  location                      = local.location
  name                          = join("-", [local.resource_prefix, "OAI", "kv"])
  public_network_access_enabled = false
  resource_group_name           = azurerm_resource_group.rgs["OAI"].name
  sku_name                      = "standard"
  tenant_id                     = data.azurerm_client_config.current.tenant_id
}

resource "azurerm_key_vault" "ops_keyvault" {
  enable_rbac_authorization     = true
  location                      = local.location
  name                          = join("-", [local.resource_prefix, "OPS", "kv"])
  public_network_access_enabled = false
  resource_group_name           = azurerm_resource_group.rgs["OPS"].name
  sku_name                      = "standard"
  tenant_id                     = data.azurerm_client_config.current.tenant_id
}

resource "azurerm_role_assignment" "openai_kv_sp_role" {
  principal_id         = data.azurerm_client_config.current.object_id
  role_definition_name = "Key Vault Secrets Officer"
  scope                = azurerm_key_vault.openai_keyvault.id
}

resource "azurerm_role_assignment" "ops_kv_sp_role" {
  principal_id         = data.azurerm_client_config.current.object_id
  role_definition_name = "Key Vault Secrets Officer"
  scope                = azurerm_key_vault.ops_keyvault.id
}

resource "azurerm_role_assignment" "ops_kv_app_config_role" {
  principal_id         = azurerm_app_configuration.app_config.identity.0.principal_id
  role_definition_name = "Key Vault Crypto Service Encryption User"
  scope                = azurerm_key_vault.openai_keyvault.id
}

resource "azurerm_private_endpoint" "oai_kv_ple" {
  location            = local.location
  name                = join("-", [local.resource_prefix, "OAI", "kv", "ple"])
  resource_group_name = azurerm_resource_group.rgs["NET"].name
  subnet_id           = azurerm_subnet.subnets["FLLMOpenAI"].id

  private_service_connection {
    is_manual_connection           = false
    name                           = join("-", [local.resource_prefix, "OAI", "kv", "psc"])
    private_connection_resource_id = azurerm_key_vault.openai_keyvault.id
    subresource_names              = ["vault"]
  }

  private_dns_zone_group {
    name                 = join("-", [local.resource_prefix, "OAI", "kv", "dzg"])
    private_dns_zone_ids = [local.private_dns_zones["privatelink.vaultcore.azure.net"].id]
  }
}

resource "azurerm_private_endpoint" "ops_kv_ple" {
  location            = local.location
  name                = join("-", [local.resource_prefix, "OPS", "kv", "ple"])
  resource_group_name = azurerm_resource_group.rgs["NET"].name
  subnet_id           = azurerm_subnet.subnets["FLLMServices"].id

  private_service_connection {
    is_manual_connection           = false
    name                           = join("-", [local.resource_prefix, "OPS", "kv", "psc"])
    private_connection_resource_id = azurerm_key_vault.ops_keyvault.id
    subresource_names              = ["vault"]
  }

  private_dns_zone_group {
    name                 = join("-", [local.resource_prefix, "OPS", "kv", "dzg"])
    private_dns_zone_ids = [local.private_dns_zones["privatelink.vaultcore.azure.net"].id]
  }
}