locals {
  aci_name = "${var.resource_prefix}-aci"
  alert = {
    cpu = {
      aggregation = "Average"
      description = "Alert on Container Instance CPU Threshold - CPU Utilization over 80% for 5 minutes"
      frequency   = "PT1M"
      metric_name = "CpuUsage"
      operator    = "GreaterThan"
      threshold   = 80
      window_size = "PT5M"
      severity    = 2
    }

    ram = {
      aggregation = "Average"
      description = "Alert on Container Instance Memory Threshold - Memory Utilization over 80% for 5 minutes."
      frequency   = "PT1M"
      metric_name = "MemoryUsage"
      operator    = "GreaterThan"
      threshold   = 80
      window_size = "PT5M"
      severity    = 2
    }
  }
}

resource "azurerm_container_group" "agent" {
  ip_address_type     = "None"
  location            = var.resource_group.location
  name                = local.aci_name
  os_type             = "Linux"
  resource_group_name = var.resource_group.name
  restart_policy      = "Always"
  subnet_ids          = [var.subnet_id]
  tags                = var.tags

  container {
    cpu    = "0.5"
    image  = "hashicorp/tfc-agent:latest"
    memory = "2"
    name   = "tfc-agent"

    environment_variables = {
      TFC_AGENT_NAME   = replace(local.aci_name, "-", "")
      TFC_AGENT_SINGLE = "true"
    }

    secure_environment_variables = {
      TFC_AGENT_TOKEN = var.tfc_agent_token
    }
  }

  diagnostics {
    log_analytics {
      log_type      = "ContainerInsights"
      workspace_id  = var.log_analytics_workspace.workspace_id
      workspace_key = var.log_analytics_workspace.primary_shared_key
    }
  }

  lifecycle {
    ignore_changes = [ip_address_type] // Azure API Bug
  }
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-aci-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_container_group.agent.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.ContainerInstance/containergroups"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}
