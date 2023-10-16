locals {
  hostname           = "www.internal.foundationallm.ai"
  location           = var.location
  location_short     = var.location_short
  private_dns_zones  = var.private_dns_zones
  resource_prefix    = upper(join("-", [local.location_short, var.resource_prefix]))
  tags               = merge(var.tags, { workspace = terraform.workspace })
  vnet_address_space = var.vnet_address_space
}

data "azurerm_client_config" "current" {}

resource "azurerm_monitor_action_group" "do_nothing" {
  name                = "${local.resource_prefix}-ag"
  resource_group_name = azurerm_resource_group.rgs["OPS"].name
  short_name          = "do-nothing"
  tags                = local.tags
}

resource "azurerm_resource_group" "rgs" {
  for_each = var.resource_groups

  location = local.location
  name     = join("-", [local.resource_prefix, each.key, "rg"])
  tags     = merge(each.value.tags, local.tags)
}

resource "azurerm_role_assignment" "keyvault_secrets_user_agw" {
  principal_id         = azurerm_user_assigned_identity.agw.principal_id
  role_definition_name = "Key Vault Secrets User"
  scope                = azurerm_resource_group.rgs["OPS"].id
}

resource "azurerm_user_assigned_identity" "agw" {
  location            = azurerm_resource_group.rgs["AppGateway"].location
  name                = "${var.resource_prefix}-agw-uai"
  resource_group_name = azurerm_resource_group.rgs["AppGateway"].name
  tags                = local.tags
}

module "ampls" {
  source = "./modules/monitor-private-link-scope"

  resource_group  = azurerm_resource_group.rgs["OPS"]
  resource_prefix = local.resource_prefix
  tags            = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["Services"].id
    private_dns_zone_ids = [
      var.private_dns_zones["privatelink.blob.core.windows.net"].id,
      var.private_dns_zones["privatelink.monitor.azure.com"].id,
    ]
  }
}

module "appconfig" {
  source = "./modules/app-config"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  encryption_keyvault_id     = module.ops_keyvault.id
  resource_group             = azurerm_resource_group.rgs["OPS"]
  resource_prefix            = local.resource_prefix
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["Services"].id
    private_dns_zone_ids = [
      var.private_dns_zones["privatelink.azconfig.io"].id,
    ]
  }
}

module "application_gateway" {
  source     = "./modules/application-gateway"
  depends_on = [azurerm_role_assignment.keyvault_secrets_user_agw]

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  backend_pool_ip_addresses  = ["10.255.255.255"] # TODO: Replace with AKS frontend IP address
  hostname                   = local.hostname
  identity_id                = azurerm_user_assigned_identity.agw.id
  key_vault_secret_id        = module.application_gateway_certificate.certificate_secret_id
  log_analytics_workspace_id = module.logs.id
  public_dns_zone            = var.public_dns_zone
  resource_group             = azurerm_resource_group.rgs["AppGateway"]
  resource_prefix            = local.resource_prefix
  subnet_id                  = azurerm_subnet.subnets["AppGateway"].id
  tags                       = local.tags
}

module "application_gateway_certificate" {
  source = "./modules/keyvault-acme-certificate"

  administrator_email = "tbd@solliance.net"
  domain              = local.hostname
  key_vault_id        = module.ops_keyvault.id
  public_dns_zone     = var.public_dns_zone
}

module "backend_aks" {
  source = "./modules/aks"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  agw_id                     = module.application_gateway.id
  aks_admin_object_id        = var.sql_admin_ad_group.object_id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rgs["APP"]
  resource_prefix            = "${local.resource_prefix}-BACKEND"
  tags                       = local.tags

  private_endpoint = {
    subnet = azurerm_subnet.subnets["FLLMServices"]
    private_dns_zone_ids = {
      aks = [
        var.private_dns_zones["privatelink.eastus.azmk8s.io"].id,
      ]
    }
  }
}

module "cosmosdb" {
  source = "./modules/cosmosdb"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rgs["FLLMStorage"]
  resource_prefix            = "${local.resource_prefix}-FLLM"
  tags                       = local.tags

  containers = {
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

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["FLLMStorage"].id
    private_dns_zone_ids = [
      var.private_dns_zones["privatelink.documents.azure.com"].id,
    ]
  }
}

module "data_cosmosdb" {
  source = "./modules/cosmosdb"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rgs["Data"]
  resource_prefix            = "${local.resource_prefix}-DATA"
  tags                       = local.tags

  containers = {
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

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["Datasources"].id
    private_dns_zone_ids = [
      var.private_dns_zones["privatelink.documents.azure.com"].id,
    ]
  }
}

module "ha_openai" {
  source = "./modules/ha-openai"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  instance_count             = 4
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rgs["OAI"]
  resource_prefix            = "${local.resource_prefix}-OAI"
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["FLLMOpenAI"].id
    apim_private_dns_zones = [
      local.private_dns_zones["azure-api.net"],
      local.private_dns_zones["developer.azure-api.net"],
      local.private_dns_zones["management.azure-api.net"],
      local.private_dns_zones["portal.azure-api.net"],
      local.private_dns_zones["scm.azure-api.net"]
    ]
    kv_private_dns_zone_ids = [
      local.private_dns_zones["privatelink.vaultcore.azure.net"].id
    ]
    openai_private_dns_zone_ids = [
      local.private_dns_zones["privatelink.openai.azure.com"].id
    ]
  }
}

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

module "search" {
  source = "./modules/search"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rgs["VEC"]
  resource_prefix            = local.resource_prefix
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["Vectorization"].id
    private_dns_zone_ids = [
      var.private_dns_zones["privatelink.search.windows.net"].id,
    ]
  }
}

module "sql" {
  source = "./modules/mssql-server"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rgs["Data"]
  resource_prefix            = local.resource_prefix
  sql_admin_object_id        = var.sql_admin_ad_group.object_id
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["Datasources"].id
    private_dns_zone_ids = [
      var.private_dns_zones["privatelink.database.windows.net"].id,
    ]
  }
}

module "storage" {
  source = "./modules/storage-account"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rgs["FLLMStorage"]
  resource_prefix            = "${local.resource_prefix}-FLLM-prompt"
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["FLLMStorage"].id
    private_dns_zone_ids = {
      blob  = [var.private_dns_zones["privatelink.blob.core.windows.net"].id]
      dfs   = [var.private_dns_zones["privatelink.dfs.core.windows.net"].id]
      file  = [var.private_dns_zones["privatelink.file.core.windows.net"].id]
      queue = [var.private_dns_zones["privatelink.queue.core.windows.net"].id]
      table = [var.private_dns_zones["privatelink.table.core.windows.net"].id]
      web   = [var.private_dns_zones["privatelink.azurewebsites.net"].id]
    }
  }
}

module "data_storage" {
  source = "./modules/storage-account"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rgs["Data"]
  resource_prefix            = "${local.resource_prefix}-DATA"
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["Datasources"].id
    private_dns_zone_ids = {
      blob  = [var.private_dns_zones["privatelink.blob.core.windows.net"].id]
      dfs   = [var.private_dns_zones["privatelink.dfs.core.windows.net"].id]
      file  = [var.private_dns_zones["privatelink.file.core.windows.net"].id]
      queue = [var.private_dns_zones["privatelink.queue.core.windows.net"].id]
      table = [var.private_dns_zones["privatelink.table.core.windows.net"].id]
      web   = [var.private_dns_zones["privatelink.azurewebsites.net"].id]
    }
  }
}

module "logs" {
  source = "./modules/log-analytics-workspace"

  action_group_id = azurerm_monitor_action_group.do_nothing.id
  resource_group  = azurerm_resource_group.rgs["OPS"]
  resource_prefix = local.resource_prefix
  tags            = local.tags

  azure_monitor_private_link_scope = {
    name                = module.ampls.name
    resource_group_name = azurerm_resource_group.rgs["OPS"].name
  }
}

module "tfc_agent" {
  source = "./modules/tfc-agent"

  action_group_id         = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace = module.logs
  resource_group          = azurerm_resource_group.rgs["OPS"]
  resource_prefix         = "${local.resource_prefix}-tfca"
  subnet_id               = azurerm_subnet.subnets["tfc"].id
  tags                    = local.tags
  tfc_agent_token         = var.tfc_agent_token
}
