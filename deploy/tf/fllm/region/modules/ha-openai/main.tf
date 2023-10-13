locals {
  apim_alert = {
    capacity = {
      aggregation = "Average"
      description = "Service capacity greater than 75% for 1 hour"
      frequency   = "PT1M"
      metric_name = "Capacity"
      operator    = "GreaterThan"
      threshold   = 75
      window_size = "PT1H"
      severity    = 0
    }
  }
  openai_alert = {
    availability = {
      aggregation = "Average"
      description = "Service availability less than 99% for 1 hour"
      frequency   = "PT1M"
      metric_name = "SuccessRate"
      operator    = "LessThan"
      threshold   = 99
      window_size = "PT1H"
      severity    = 0
    }
    latency = {
      aggregation = "Average"
      description = "Service latency greater than 1000ms for 1 hour"
      frequency   = "PT1M"
      metric_name = "Latency"
      operator    = "GreaterThan"
      threshold   = 1000
      window_size = "PT1H"
      severity    = 0
    }
  }
}

module "openai_keyvault" {
  source = "../keyvault"

  action_group_id            = var.action_group_id
  log_analytics_workspace_id = var.log_analytics_workspace_id
  resource_group             = var.resource_group
  resource_prefix            = var.resource_prefix
  tags                       = var.tags

  private_endpoint = {
    subnet_id            = var.private_endpoint.subnet_id
    private_dns_zone_ids = var.private_endpoint.kv_private_dns_zone_ids
  }
}

resource "azurerm_public_ip" "apim_mgmt_ip" {
  allocation_method   = "Static"
  domain_name_label   = "${lower(var.resource_prefix)}-apim"
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-pip"
  resource_group_name = var.resource_group.name
  sku                 = "Standard"
}

resource "azurerm_api_management" "apim" {
  location             = var.resource_group.location
  name                 = "${var.resource_prefix}-apim"
  public_ip_address_id = azurerm_public_ip.apim_mgmt_ip.id
  publisher_email      = var.publisher.email
  publisher_name       = var.publisher.name
  resource_group_name  = var.resource_group.name
  sku_name             = "Premium_1"
  virtual_network_type = "Internal"
  tags                 = var.tags

  identity {
    type = "SystemAssigned"
  }

  virtual_network_configuration {
    subnet_id = var.private_endpoint.subnet_id
  }
}

resource "azurerm_api_management_api" "api" {
  api_management_name = azurerm_api_management.apim.name
  display_name        = "HA OpenAI"
  name                = "${var.resource_prefix}-api"
  path                = "openai"
  protocols           = ["https"]
  resource_group_name = var.resource_group.name
  revision            = "1"

  import {
    content_format = "openapi+json-link"
    content_value  = "https://raw.githubusercontent.com/Azure/azure-rest-api-specs/main/specification/cognitiveservices/data-plane/AzureOpenAI/inference/stable/2023-05-15/inference.json"
  }
}

resource "azurerm_cognitive_account" "openai" {
  count = var.instance_count

  custom_subdomain_name         = "${lower(var.resource_prefix)}-${count.index}"
  kind                          = "OpenAI"
  location                      = var.resource_group.location
  name                          = "${var.resource_prefix}-${count.index}-openai"
  public_network_access_enabled = false
  resource_group_name           = var.resource_group.name
  sku_name                      = "S0"
  tags                          = var.tags
}

resource "azurerm_cognitive_deployment" "deployment" {
  count = length(azurerm_cognitive_account.openai)

  cognitive_account_id = azurerm_cognitive_account.openai[count.index].id
  name                 = "completions"

  model {
    format  = "OpenAI"
    name    = "gpt-35-turbo"
    version = "0301"
  }

  scale {
    capacity = "60"
    type     = "Standard"
  }
}

resource "azurerm_role_assignment" "openai_apim" {
  principal_id         = azurerm_api_management.apim.identity.0.principal_id
  role_definition_name = "Key Vault Secrets User"
  scope                = module.openai_keyvault.id
}

resource "azurerm_api_management_named_value" "openai_primary_key" {
  count = length(azurerm_key_vault_secret.openai_primary_key)

  api_management_name = azurerm_api_management.apim.name
  display_name        = azurerm_key_vault_secret.openai_primary_key[count.index].name
  name                = azurerm_key_vault_secret.openai_primary_key[count.index].name
  resource_group_name = var.resource_group.name
  secret              = true

  value_from_key_vault {
    secret_id = azurerm_key_vault_secret.openai_primary_key[count.index].id
  }

  depends_on = [
    azurerm_role_assignment.openai_apim
  ]
}

resource "azurerm_api_management_named_value" "openai_secondary_key" {
  count = length(azurerm_key_vault_secret.openai_secondary_key)

  api_management_name = azurerm_api_management.apim.name
  display_name        = azurerm_key_vault_secret.openai_secondary_key[count.index].name
  name                = azurerm_key_vault_secret.openai_secondary_key[count.index].name
  resource_group_name = var.resource_group.name
  secret              = true

  value_from_key_vault {
    secret_id = azurerm_key_vault_secret.openai_secondary_key[count.index].id
  }

  depends_on = [
    azurerm_role_assignment.openai_apim
  ]
}

resource "azurerm_key_vault_secret" "openai_primary_key" {
  count = length(azurerm_cognitive_account.openai)

  key_vault_id = module.openai_keyvault.id
  name         = "${var.resource_prefix}-${count.index}-primarykey"
  value        = azurerm_cognitive_account.openai[count.index].primary_access_key
}

resource "azurerm_key_vault_secret" "openai_secondary_key" {
  count = length(azurerm_cognitive_account.openai)

  key_vault_id = module.openai_keyvault.id
  name         = "${var.resource_prefix}-${count.index}-secondarykey"
  value        = azurerm_cognitive_account.openai[count.index].secondary_access_key
}

resource "azurerm_api_management_backend" "backends" {
  count = length(azurerm_cognitive_account.openai)

  api_management_name = azurerm_api_management.apim.name
  name                = "${var.resource_prefix}-${count.index}-backend"
  protocol            = "http"
  resource_group_name = var.resource_group.name
  url                 = join("", [azurerm_cognitive_account.openai[count.index].endpoint, "openai"])

  credentials {
    header = {
      "api-key" = join(",", [
        "{{${azurerm_api_management_named_value.openai_primary_key[count.index].name}}}",
        "{{${azurerm_api_management_named_value.openai_secondary_key[count.index].name}}}"
      ])
    }
  }

  tls {
    validate_certificate_chain = true
    validate_certificate_name  = true
  }
}

locals {
  inbound_choices = join("",
    [for index, backend in azurerm_api_management_backend.backends : <<XML
      <when condition="@(context.Variables.GetValueOrDefault<int>("backendId") == ${index + 1})">
        <set-backend-service backend-id="${backend.name}"/>
      </when>
XML
    ]
  )
}

resource "azurerm_api_management_api_policy" "api_policy" {
  api_management_name = azurerm_api_management_api.api.api_management_name
  api_name            = azurerm_api_management_api.api.name
  resource_group_name = var.resource_group.name

  xml_content = <<XML
  <policies>
  <inbound>
    <base/>
    <set-variable name="backendId" value="@(new Random(context.RequestId.GetHashCode()).Next(1, ${length(azurerm_cognitive_account.openai) + 1}))" />
    <choose>
${local.inbound_choices}
      <otherwise>
      <!-- Should never happen, but you never know ;) -->
        <return-response>
          <set-status code="500" reason="InternalServerError" />
          <set-header name="Microsoft-Azure-Api-Management-Correlation-Id" exists-action="override">
            <value>@{return Guid.NewGuid().ToString();}</value>
          </set-header>
          <set-body>A gateway-related error occurred while processing the request.</set-body>
        </return-response>
      </otherwise>
    </choose>
  </inbound>
  <backend>
    <base/>
  </backend>
  <outbound>
    <base/>
  </outbound>
  <on-error>
    <base/>
  </on-error>
</policies>
XML
}

resource "azurerm_monitor_metric_alert" "apim_alert" {
  for_each = local.apim_alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-apim-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_api_management.apim.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.ApiManagement/service"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}

locals {
  openai_alert_map = flatten([
    for index, account in azurerm_cognitive_account.openai : [
      for key, alert in local.openai_alert : {
        name        = "openai${index}-${key}"
        account     = account
        aggregation = alert.aggregation
        description = alert.description
        frequency   = alert.frequency
        metric_name = alert.metric_name
        operator    = alert.operator
        threshold   = alert.threshold
        window_size = alert.window_size
        severity    = alert.severity
      }
    ]
  ])
}

resource "azurerm_monitor_metric_alert" "openai_alert" {
  for_each = {
    for alert in local.openai_alert_map : alert.name => alert
  }

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-openai-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [each.value.account.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.CognitiveServices/account"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}

resource "azurerm_private_dns_a_record" "apim" {
  count = length(var.private_endpoint.apim_private_dns_zones)

  name                = lower(azurerm_api_management.apim.name)
  records             = azurerm_api_management.apim.private_ip_addresses
  resource_group_name = var.private_endpoint.apim_private_dns_zones[count.index].resource_group_name
  ttl                 = 0
  zone_name           = var.private_endpoint.apim_private_dns_zones[count.index].name
}

resource "azurerm_private_endpoint" "ple" {
  count = length(azurerm_cognitive_account.openai)

  location            = var.resource_group.location
  name                = "${var.resource_prefix}-${count.index}-openai-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet_id
  tags                = var.tags

  private_dns_zone_group {
    name                 = "openai"
    private_dns_zone_ids = var.private_endpoint.openai_private_dns_zone_ids
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-${count.index}-openai-connection"
    private_connection_resource_id = azurerm_cognitive_account.openai[count.index].id
    subresource_names              = ["account"]
  }
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = merge({
    apim = {
      id = azurerm_api_management.apim.id
    }
    },
    { for index, service in azurerm_cognitive_account.openai :
      "openai-${index}" => {
        id = service.id
      }
  })
}
