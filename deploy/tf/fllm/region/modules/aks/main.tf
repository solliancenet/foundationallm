locals {
  alert = {
    node_cpu = {
      aggregation = "Average"
      description = "Node CPU utilization greater than 75% for 1 hour"
      frequency   = "PT1M"
      metric_name = "mode_cpu_usage_percentage"
      operator    = "GreaterThan"
      threshold   = 75
      window_size = "PT1H"
      severity    = 0
    }
  }
}

data "azurerm_client_config" "current" {}

resource "azurerm_user_assigned_identity" "aks_mi" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-aks-mi"
  resource_group_name = var.resource_group.name
}

resource "azurerm_role_assignment" "aks_mi" {
  scope                = var.private_endpoint.private_dns_zone_ids["aks"][0]
  principal_id         = azurerm_user_assigned_identity.aks_mi.principal_id
  role_definition_name = "Private DNS Zone Contributor"
}

resource "azurerm_kubernetes_cluster" "main" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-aks"
  resource_group_name = var.resource_group.name

  dns_prefix_private_cluster = "${var.resource_prefix}-aks"

  aci_connector_linux {
    subnet_name = var.private_endpoint.subnet.name
  }

  automatic_channel_upgrade = "stable"

  auto_scaler_profile {}

  azure_active_directory_role_based_access_control {
    managed   = true
    tenant_id = data.azurerm_client_config.current.tenant_id
    admin_group_object_ids = [
      data.azurerm_client_config.current.object_id,
      var.aks_admin_object_id
    ]
    azure_rbac_enabled = true
  }

  azure_policy_enabled = true

  ingress_application_gateway {
    gateway_id = var.agw_id
  }

  oidc_issuer_enabled = true

  oms_agent {
    log_analytics_workspace_id = var.log_analytics_workspace_id
  }

  private_cluster_enabled = true

  private_dns_zone_id = var.private_endpoint.private_dns_zone_ids["aks"][0]

  workload_identity_enabled = true

  role_based_access_control_enabled = true

  sku_tier = "Standard"

  default_node_pool {
    name                = "default"
    vm_size             = "Standard_D2_v2"
    type                = "VirtualMachineScaleSets"
    pod_subnet_id       = var.private_endpoint.subnet.id
    enable_auto_scaling = true
    max_count           = 5
    min_count           = 2
  }

  identity {
    type = "UserAssigned"
    identity_ids = [
      azurerm_user_assigned_identity.aks_mi.id
    ]
  }

  microsoft_defender {
    log_analytics_workspace_id = var.log_analytics_workspace_id
  }

  tags = var.tags

  depends_on = [
    azurerm_role_assignment.aks_mi
  ]
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-aks-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_kubernetes_cluster.main.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.ContainerService/managedClusters"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}

resource "azurerm_private_endpoint" "ple" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-aks-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet.id
  tags                = var.tags

  private_dns_zone_group {
    name                 = "aks"
    private_dns_zone_ids = var.private_endpoint.private_dns_zone_ids.aks
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-aks-connection"
    private_connection_resource_id = azurerm_kubernetes_cluster.main.id
    subresource_names              = ["management"]
  }
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    aks = {
      id = azurerm_kubernetes_cluster.main.id
    }
  }
}