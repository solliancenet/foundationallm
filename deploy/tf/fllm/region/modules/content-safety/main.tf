locals {
  alert = {

  }
}

resource "azurerm_cognitive_account" "main" {
  kind                          = "ContentModerator"
  location                      = var.resource_group.location
  name                          = "${var.resource_prefix}-content-safety"
  resource_group_name           = var.resource_group.name
  sku_name                      = "S0"
  custom_subdomain_name         = "${var.resource_prefix}-content-safety"
  public_network_access_enabled = false

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-content-safety-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_cognitive_account.main.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.CognitiveServices/accounts"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}


resource "azurerm_private_endpoint" "ple" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-content-safety-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet_id
  tags                = var.tags

  private_dns_zone_group {
    name                 = "contentSafety"
    private_dns_zone_ids = var.private_endpoint.private_dns_zone_ids
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-content-safety-connection"
    private_connection_resource_id = azurerm_cognitive_account.main.id
    subresource_names              = ["account"]
  }
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    contentSafety = {
      id = azurerm_cognitive_account.main.id
    }
  }
}
