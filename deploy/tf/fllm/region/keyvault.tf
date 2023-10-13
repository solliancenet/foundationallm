module "ops_keyvault" {
  source = "./modules/keyvault"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rgs["OPS"]
  resource_prefix            = "${local.resource_prefix}-OPS"
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["Services"].id
    private_dns_zone_ids = [
      local.private_dns_zones["privatelink.vaultcore.azure.net"].id
    ]
  }
}


