locals {
  openai = {
    OAI1 = {}
    OAI2 = {}
    OAI3 = {}
    OAI4 = {}
  }
}

resource "azurerm_cognitive_account" "openai" {
  for_each = local.openai

  name                          = join("-", [local.resource_prefix, each.key, "openai"])
  location                      = local.location
  resource_group_name           = azurerm_resource_group.rgs["OAI"].name
  kind                          = "OpenAI"
  custom_subdomain_name         = lower(join("-", [local.resource_prefix, each.key]))
  public_network_access_enabled = false

  sku_name = "S0"

  tags = local.tags
}

resource "azurerm_key_vault_secret" "openai_primary_key" {
  for_each = azurerm_cognitive_account.openai

  name         = join("-", [local.resource_prefix, each.key, "primarykey"])
  value        = each.value.primary_access_key
  key_vault_id = azurerm_key_vault.openai_keyvault.id
}

resource "azurerm_key_vault_secret" "openai_secondary_key" {
  for_each = azurerm_cognitive_account.openai

  name         = join("-", [local.resource_prefix, each.key, "secondarykey"])
  value        = each.value.secondary_access_key
  key_vault_id = azurerm_key_vault.openai_keyvault.id
}

resource "azurerm_private_endpoint" "openai_ple" {
  for_each = azurerm_cognitive_account.openai

  name                = join("-", [local.resource_prefix, each.key, "openai", "ple"])
  location            = local.location
  resource_group_name = azurerm_resource_group.rgs["NET"].name
  subnet_id           = azurerm_subnet.subnets["FLLMOpenAI"].id

  private_service_connection {
    is_manual_connection           = false
    name                           = join("-", [local.resource_prefix, each.key, "openai", "psc"])
    private_connection_resource_id = each.value.id
    subresource_names              = ["account"]
  }

  private_dns_zone_group {
    name                 = join("-", [local.resource_prefix, each.key, "openai", "dzg"])
    private_dns_zone_ids = [local.private_dns_zones["privatelink.openai.azure.com"].id]
  }
}

resource "azurerm_public_ip" "openai_apim_mgmt_ip" {
  allocation_method   = "Static"
  location            = local.location
  name                = join("-", [local.resource_prefix, "OAI", "pip"])
  resource_group_name = azurerm_resource_group.rgs["NET"].name
  sku                 = "Standard"
  domain_name_label   = "openai-apim-mgmt"
}

resource "azurerm_api_management" "openai_apim" {
  name                 = join("-", [local.resource_prefix, "OAI", "apim"])
  location             = local.location
  resource_group_name  = azurerm_resource_group.rgs["OAI"].name
  publisher_name       = "FoundationaLLM"
  publisher_email      = "ciprian@solliance.net"
  sku_name             = "Developer_1"
  virtual_network_type = "Internal"
  public_ip_address_id = azurerm_public_ip.openai_apim_mgmt_ip.id

  virtual_network_configuration {
    subnet_id = azurerm_subnet.subnets["FLLMOpenAI"].id
  }

  identity {
    type = "SystemAssigned"
  }

  tags = local.tags
}

#resource "azurerm_role_assignment" "openai_apim" {
#  principal_id         = azurerm_api_management.openai_apim.identity.0.principal_id
#  scope                = azurerm_key_vault.openai_keyvault.id
#  role_definition_name = "Key Vault Secrets User"
#}
#
#resource "azurerm_api_management_named_value" "openai_primary_key" {
#  for_each = azurerm_key_vault_secret.openai_primary_key
#
#  name                = each.value.name
#  resource_group_name = azurerm_api_management.openai_apim.resource_group_name
#  api_management_name = azurerm_api_management.openai_apim.name
#  display_name        = each.value.name
#  value_from_key_vault {
#    secret_id = each.value.id
#  }
#}
#
#resource "azurerm_api_management_named_value" "openai_secondary_key" {
#  for_each = azurerm_key_vault_secret.openai_secondary_key
#
#  name                = each.value.name
#  resource_group_name = azurerm_api_management.openai_apim.resource_group_name
#  api_management_name = azurerm_api_management.openai_apim.name
#  display_name        = each.value.name
#  value_from_key_vault {
#    secret_id = each.value.id
#  }
#}

resource "azurerm_api_management_api" "openai_api" {
  name                = join("-", [local.resource_prefix, "OAI", "api"])
  resource_group_name = azurerm_resource_group.rgs["OAI"].name
  api_management_name = azurerm_api_management.openai_apim.name
  revision            = "1"
  display_name        = "HA OpenAI"
  path                = "openai"
  protocols           = ["https"]
}