locals {
  alert = {
    storageUsage = {
      aggregation = "Maximum"
      description = "Service maximum storage usage greater than 75% for 1 hour"
      frequency   = "PT1M"
      metric_name = "DailyStorageUsage"
      operator    = "GreaterThan"
      threshold   = 75
      window_size = "PT1H"
      severity    = 0
    }
    latency = {
      aggregation = "Average"
      description = "Service request latency greater than 1000ms for 1 hour"
      frequency   = "PT1M"
      metric_name = "DailyStorageUsage"
      operator    = "GreaterThan"
      threshold   = 1000
      window_size = "PT1H"
      severity    = 0
    }
  }
}

resource "azurerm_app_configuration" "main" {

  location                   = var.resource_group.location
  name                       = "${var.resource_prefix}-appconfig"
  public_network_access      = "Disabled"
  purge_protection_enabled   = true
  resource_group_name        = var.resource_group.name
  soft_delete_retention_days = 1
  sku                        = "standard"

  identity {
    type = "UserAssigned"
    identity_ids = [
      azurerm_user_assigned_identity.app_config_mi.id
    ]
  }

  encryption {
    key_vault_key_identifier = azurerm_key_vault_key.app_config_key.id
    identity_client_id       = azurerm_user_assigned_identity.app_config_mi.client_id
  }

  tags = var.tags

  depends_on = [
    azurerm_key_vault_key.app_config_key,
    azurerm_role_assignment.app_config_kv_role
  ]
}

resource "azurerm_user_assigned_identity" "app_config_mi" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-mi"
  resource_group_name = var.resource_group.name
}

resource "azurerm_key_vault_key" "app_config_key" {
  name         = "${var.resource_prefix}-key"
  key_vault_id = var.encryption_keyvault_id
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

resource "azurerm_role_assignment" "app_config_kv_role" {
  principal_id         = azurerm_user_assigned_identity.app_config_mi.principal_id
  role_definition_name = "Key Vault Crypto Service Encryption User"
  scope                = var.encryption_keyvault_id
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-appconfig-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_app_configuration.main.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.AppConfiguration/configurationStores"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}

resource "azurerm_private_endpoint" "ple" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-appconfig-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet_id
  tags                = var.tags

  private_dns_zone_group {
    name                 = "appconfig"
    private_dns_zone_ids = var.private_endpoint.private_dns_zone_ids
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-appconfig-connection"
    private_connection_resource_id = azurerm_app_configuration.main.id
    subresource_names              = ["configurationStores"]
  }
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    appconfig = {
      id = azurerm_app_configuration.main.id
    }
  }
}
