locals {
  selected_categories = { for k, v in data.azurerm_monitor_diagnostic_categories.categories :
    k => {
      id    = var.monitored_services[k].id
      table = var.monitored_services[k].table
      logs  = [for l in v.logs : l if contains(var.monitored_services[k].include, l) || length(var.monitored_services[k].include) == 0]
    }
  }
}

data "azurerm_monitor_diagnostic_categories" "categories" {
  for_each = var.monitored_services

  resource_id = each.value.id
}

resource "azurerm_monitor_diagnostic_setting" "setting" {
  for_each = local.selected_categories

  name                           = "diag-${each.key}"
  target_resource_id             = each.value.id
  log_analytics_workspace_id     = var.log_analytics_workspace_id
  log_analytics_destination_type = each.value.table == "None" ? null : each.value.table

  dynamic "enabled_log" {
    for_each = each.value.logs

    content {
      category = enabled_log.value
    }
  }

  dynamic "metric" {
    for_each = data.azurerm_monitor_diagnostic_categories.categories[each.key].metrics

    content {
      category = metric.value
      enabled  = true
    }
  }

  lifecycle {
    ignore_changes = [
      log_analytics_destination_type # Azure API Bug or maybe TF provider bug
    ]
  }
}