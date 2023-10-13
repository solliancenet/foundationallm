locals {
  alert = {
    latency = {
      aggregation = "Average"
      description = "Service latency greater than 5s for 1 hour"
      frequency   = "PT1M"
      metric_name = "SearchLatency"
      operator    = "GreaterThan"
      threshold   = 5
      window_size = "PT1H"
      severity    = 0
    }
    throttling = {
      aggregation = "Average"
      description = "Service throttled search queries greater than 25% for 1 hour"
      frequency   = "PT1M"
      metric_name = "SearchLatency"
      operator    = "GreaterThan"
      threshold   = 25
      window_size = "PT1H"
      severity    = 0
    }
  }
}

resource "azurerm_search_service" "main" {
  location                      = var.resource_group.location
  name                          = "${lower(join("", split("-", var.resource_prefix)))}search"
  resource_group_name           = var.resource_group.name
  sku                           = "standard"
  public_network_access_enabled = false
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-search-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_search_service.main.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.Search/searchServices"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}

resource "azurerm_private_endpoint" "ple" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-search-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet_id
  tags                = var.tags

  private_dns_zone_group {
    name                 = "search"
    private_dns_zone_ids = var.private_endpoint.private_dns_zone_ids
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-search-connection"
    private_connection_resource_id = azurerm_search_service.main.id
    subresource_names              = ["searchService"]
  }
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    search = {
      id = azurerm_search_service.main.id
    }
  }
}
