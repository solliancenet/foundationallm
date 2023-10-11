resource "azurerm_monitor_private_link_scope" "main" {
  name                = "${var.resource_prefix}-ampls"
  resource_group_name = var.resource_group.name
  tags                = var.tags
}

resource "azurerm_private_endpoint" "main" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-azuremonitor-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet_id
  tags                = var.tags

  private_dns_zone_group {
    name                 = "azuremonitor"
    private_dns_zone_ids = var.private_endpoint.private_dns_zone_ids
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-azuremonitor-connection"
    private_connection_resource_id = azurerm_monitor_private_link_scope.main.id
    subresource_names              = ["azuremonitor"]
  }
}
