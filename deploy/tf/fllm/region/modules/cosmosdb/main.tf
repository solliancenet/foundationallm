locals {
  alert = {}
  container = {
    embedding = {
      partition_key_path = "/id"
      max_throughput     = 1000
    }
    completions = {
      partition_key_path = "/sessionId"
      max_throughput     = 1000
    }
    product = {
      partition_key_path = "/categoryId"
      max_throughput     = 1000
    }
    customer = {
      partition_key_path = "/customerId"
      max_throughput     = 1000
    }
    leases = {
      partition_key_path = "/id"
      max_throughput     = 1000
    }
  }
}

resource "azurerm_cosmosdb_account" "main" {
  kind                = "GlobalDocumentDB"
  location            = var.resource_group.location
  name                = lower("${var.resource_prefix}-cdb")
  offer_type          = "Standard"
  resource_group_name = var.resource_group.name

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    failover_priority = 0
    location          = var.resource_group.location
  }
}

resource "azurerm_cosmosdb_sql_database" "db" {
  account_name        = azurerm_cosmosdb_account.main.name
  name                = "database"
  resource_group_name = var.resource_group.name
}

resource "azurerm_cosmosdb_sql_container" "container" {
  for_each = local.container

  account_name          = azurerm_cosmosdb_account.main.name
  database_name         = azurerm_cosmosdb_sql_database.db.name
  name                  = each.key
  partition_key_path    = each.value.partition_key_path
  partition_key_version = 2
  resource_group_name   = var.resource_group.name

  autoscale_settings {
    max_throughput = each.value.max_throughput
  }
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-cdb-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_cosmosdb_account.main.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.DocumentDB/DatabaseAccounts"
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
    private_connection_resource_id = azurerm_cosmosdb_account.main.id
    subresource_names              = ["Sql"]
  }
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    cdb = {
      id = azurerm_cosmosdb_account.main.id
    }
  }
}


