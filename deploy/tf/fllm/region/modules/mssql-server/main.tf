locals {
  alert = {
    cpu = {
      aggregation = "Average"
      description = "Service CPU utilization greater than 75% for 1 hour"
      frequency   = "PT1M"
      metric_name = "ServiceAvailability"
      operator    = "GreaterThan"
      threshold   = 75
      window_size = "PT1H"
      severity    = 0
    }
  }
}

data "azurerm_client_config" "current" {}

resource "azurerm_mssql_server" "main" {

  location                      = var.resource_group.location
  name                          = lower(join("", split("-", "${var.resource_prefix}-mssql")))
  minimum_tls_version           = "1.2"
  public_network_access_enabled = false
  resource_group_name           = var.resource_group.name
  version                       = "12.0"

  azuread_administrator {
    login_username              = "ADAdmin"
    object_id                   = data.azurerm_client_config.current.object_id
    azuread_authentication_only = true
  }

  tags = var.tags
}

resource "azurerm_mssql_elasticpool" "main" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-mssql-ep"
  resource_group_name = var.resource_group.name
  server_name         = azurerm_mssql_server.main.name
  license_type        = "LicenseIncluded"
  max_size_gb         = 32

  sku {
    name     = "GP_Gen5"
    capacity = 4
    tier     = "GeneralPurpose"
    family   = "Gen5"
  }

  per_database_settings {
    max_capacity = 4
    min_capacity = 0.25
  }

  tags = var.tags
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-mssql-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_mssql_server.main.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.Sql/servers/elasticpools"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}

resource "azurerm_private_endpoint" "ple" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-sql-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet_id
  tags                = var.tags

  private_dns_zone_group {
    name                 = "sql"
    private_dns_zone_ids = var.private_endpoint.private_dns_zone_ids
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-sql-connection"
    private_connection_resource_id = azurerm_mssql_server.main.id
    subresource_names              = ["SqlServer"]
  }
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    mssql = {
      id = azurerm_mssql_elasticpool.main.id
    }
  }
}
