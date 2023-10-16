resource "azurerm_application_insights" "main" {
  application_type           = "web"
  internet_ingestion_enabled = true
  internet_query_enabled     = true
  location                   = var.resource_group.location
  name                       = "${var.resource_prefix}-ai"
  resource_group_name        = var.resource_group.name
  retention_in_days          = 30
  tags                       = var.tags
  workspace_id               = var.log_analytics_workspace_id
}

resource "azurerm_monitor_private_link_scoped_service" "ampls" {
  linked_resource_id  = azurerm_application_insights.main.id
  name                = "${var.resource_prefix}-ai-amplss"
  resource_group_name = var.azure_monitor_private_link_scope.resource_group_name
  scope_name          = var.azure_monitor_private_link_scope.name
}