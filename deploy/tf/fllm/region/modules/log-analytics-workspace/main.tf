locals {
  alert = {
    issues = {
      display_name                             = "Operational issues - ${azurerm_log_analytics_workspace.workspace.name}"
      evaluation_frequency                     = "P1D"
      minimum_failing_periods_to_trigger_alert = 1
      number_of_evaluation_periods             = 1
      operator                                 = "GreaterThan"
      query                                    = "_LogOperation | where Level == \"Warning\""
      resource_id_column                       = "_ResourceId"
      severity                                 = 3
      threshold                                = 0.0
      time_aggregation_method                  = "Count"
      window_duration                          = "P1D"
    }

    rate-limit = {
      display_name                             = "Data ingestion is exceeding the ingestion rate limit - ${azurerm_log_analytics_workspace.workspace.name}"
      evaluation_frequency                     = "PT5M"
      minimum_failing_periods_to_trigger_alert = 1
      number_of_evaluation_periods             = 1
      operator                                 = "GreaterThan"
      query                                    = "_LogOperation | where Category == \"Ingestion\" | where Operation has \"Ingestion rate\""
      resource_id_column                       = "_ResourceId"
      severity                                 = 2
      threshold                                = 0.0
      time_aggregation_method                  = "Count"
      window_duration                          = "PT5M"
    }

    ingestion-cap = {
      display_name                             = "Data ingestion has hit the daily cap - ${azurerm_log_analytics_workspace.workspace.name}"
      evaluation_frequency                     = "PT5M"
      minimum_failing_periods_to_trigger_alert = 1
      number_of_evaluation_periods             = 1
      operator                                 = "GreaterThan"
      query                                    = "_LogOperation | where Category == \"Ingestion\" | where Operation has \"Data collection\""
      resource_id_column                       = "_ResourceId"
      severity                                 = 2
      threshold                                = 0.0
      time_aggregation_method                  = "Count"
      window_duration                          = "PT5M"
    }
  }
}

resource "azurerm_log_analytics_solution" "solution" {
  for_each = var.solutions

  location              = var.resource_group.location
  resource_group_name   = var.resource_group.name
  solution_name         = each.key
  tags                  = var.tags
  workspace_name        = azurerm_log_analytics_workspace.workspace.name
  workspace_resource_id = azurerm_log_analytics_workspace.workspace.id

  plan {
    publisher = each.value.publisher
    product   = each.value.product
  }
}

resource "azurerm_log_analytics_workspace" "workspace" {
  internet_ingestion_enabled = true
  internet_query_enabled     = true
  location                   = var.resource_group.location
  name                       = "${var.resource_prefix}-la"
  resource_group_name        = var.resource_group.name
  retention_in_days          = 30
  sku                        = "PerGB2018"
  tags                       = var.tags
}

resource "azurerm_monitor_data_collection_rule" "dcr" {
  description         = "Data collection rule for VM Insights."
  location            = var.resource_group.location
  name                = "MSVMI-${azurerm_log_analytics_workspace.workspace.name}"
  resource_group_name = var.resource_group.name
  tags                = var.tags

  data_flow {
    destinations = ["VMInsightsPerf-Logs-Dest"]
    streams      = ["Microsoft-InsightsMetrics"]
  }

  data_flow {
    destinations = ["VMInsightsPerf-Logs-Dest"]
    streams      = ["Microsoft-ServiceMap"]
  }

  data_sources {
    extension {
      extension_name = "DependencyAgent"
      name           = "DependencyAgentDataSource"
      streams        = ["Microsoft-ServiceMap"]
    }
    performance_counter {
      counter_specifiers            = ["\\VmInsights\\DetailedMetrics"]
      name                          = "VMInsightsPerfCounters"
      sampling_frequency_in_seconds = 60
      streams                       = ["Microsoft-InsightsMetrics"]
    }
  }

  destinations {
    log_analytics {
      name                  = "VMInsightsPerf-Logs-Dest"
      workspace_resource_id = azurerm_log_analytics_workspace.workspace.id
    }
  }
}

resource "azurerm_monitor_private_link_scoped_service" "ampls" {
  linked_resource_id  = azurerm_log_analytics_workspace.workspace.id
  name                = "${var.resource_prefix}-la-amplss"
  resource_group_name = var.azure_monitor_private_link_scope.resource_group_name
  scope_name          = var.azure_monitor_private_link_scope.name
}

resource "azurerm_monitor_scheduled_query_rules_alert_v2" "alert" {
  for_each = local.alert

  display_name         = each.value.display_name
  evaluation_frequency = each.value.evaluation_frequency
  location             = var.resource_group.location
  name                 = "${var.resource_prefix}-la-${each.key}-alert"
  resource_group_name  = var.resource_group.name
  scopes               = [azurerm_log_analytics_workspace.workspace.id]
  tags                 = var.tags
  severity             = each.value.severity
  window_duration      = each.value.window_duration

  action {
    action_groups = [var.action_group_id]
  }

  criteria {
    operator                = each.value.operator
    query                   = each.value.query
    resource_id_column      = each.value.resource_id_column
    threshold               = each.value.threshold
    time_aggregation_method = each.value.time_aggregation_method

    failing_periods {
      minimum_failing_periods_to_trigger_alert = each.value.minimum_failing_periods_to_trigger_alert
      number_of_evaluation_periods             = each.value.number_of_evaluation_periods
    }
  }
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = azurerm_log_analytics_workspace.workspace.id

  monitored_services = {
    la = {
      id = azurerm_log_analytics_workspace.workspace.id
    }
  }
}