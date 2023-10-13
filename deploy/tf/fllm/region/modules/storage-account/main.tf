locals {
  resource_prefix_compact = lower(replace(var.resource_prefix, "-", ""))

  alert = {
    availability = {
      aggregation = "Average"
      description = "Alert on Storage Account Threshold - Account availability less than 99% for 5 minutes"
      frequency   = "PT1M"
      metric_name = "Availability"
      operator    = "LessThan"
      severity    = 1
      threshold   = 99
      window_size = "PT5M"
    }
  }
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-sa-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_storage_account.main.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.Storage/storageaccounts"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}

resource "azurerm_private_endpoint" "ple" {
  for_each = var.private_endpoint.private_dns_zone_ids

  location            = var.resource_group.location
  name                = "${var.resource_prefix}-${each.key}-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet_id
  tags                = var.tags

  private_dns_zone_group {
    name                 = each.key
    private_dns_zone_ids = each.value
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-${each.key}-connection"
    private_connection_resource_id = azurerm_storage_account.main.id
    subresource_names              = [each.key]
  }
}

resource "azurerm_storage_account" "main" {
  account_replication_type          = "LRS"
  account_tier                      = "Standard"
  allow_nested_items_to_be_public   = false
  enable_https_traffic_only         = true
  infrastructure_encryption_enabled = true
  location                          = var.resource_group.location
  min_tls_version                   = "TLS1_2"
  name                              = "${local.resource_prefix_compact}sa"
  public_network_access_enabled     = false
  resource_group_name               = var.resource_group.name
  tags                              = var.tags

  blob_properties {
    container_delete_retention_policy { days = 30 }
    delete_retention_policy { days = 30 }
    versioning_enabled = true
  }

  identity {
    type = "SystemAssigned"
  }

  network_rules {
    bypass         = ["AzureServices", "Logging", "Metrics"]
    default_action = "Deny"
  }
}

resource "azurerm_storage_container" "container" {
  depends_on = [azurerm_private_endpoint.ple]
  for_each   = toset(var.containers)

  name                  = each.key
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

resource "azurerm_storage_share" "share" {
  depends_on = [azurerm_private_endpoint.ple]
  for_each   = var.shares

  enabled_protocol     = "SMB"
  name                 = each.key
  quota                = coalesce(each.value.quota, 50)
  storage_account_name = azurerm_storage_account.main.name
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    blobs = {
      id    = "${azurerm_storage_account.main.id}/blobServices/default/"
      table = "None"
    }
    files = {
      id    = "${azurerm_storage_account.main.id}/fileServices/default/"
      table = "None"
    }
  }
}
