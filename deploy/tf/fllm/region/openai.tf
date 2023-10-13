locals {
  openai = {
    OAI1 = {}
    OAI2 = {}
    OAI3 = {}
    OAI4 = {}
  }
}

module "openai_keyvault" {
  source = "./modules/keyvault"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rgs["OAI"]
  resource_prefix            = "${local.resource_prefix}-OAI"
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["FLLMOpenAI"].id
    private_dns_zone_ids = [
      local.private_dns_zones["privatelink.vaultcore.azure.net"].id
    ]
  }
}

resource "azurerm_api_management" "openai_apim" {
  location             = local.location
  name                 = join("-", [local.resource_prefix, "OAI", "apim"])
  public_ip_address_id = azurerm_public_ip.openai_apim_mgmt_ip.id
  publisher_email      = "ciprian@solliance.net"
  publisher_name       = "FoundationaLLM"
  resource_group_name  = azurerm_resource_group.rgs["OAI"].name
  sku_name             = "Developer_1"
  virtual_network_type = "Internal"
  tags                 = local.tags

  identity {
    type = "SystemAssigned"
  }

  virtual_network_configuration {
    subnet_id = azurerm_subnet.subnets["FLLMOpenAI"].id
  }
}

resource "azurerm_api_management_api" "openai_api" {
  api_management_name = azurerm_api_management.openai_apim.name
  display_name        = "HA OpenAI"
  name                = join("-", [local.resource_prefix, "OAI", "api"])
  path                = "openai"
  protocols           = ["https"]
  resource_group_name = azurerm_resource_group.rgs["OAI"].name
  revision            = "1"

  import {
    content_format = "openapi+json-link"
    content_value  = "https://raw.githubusercontent.com/Azure/azure-rest-api-specs/main/specification/cognitiveservices/data-plane/AzureOpenAI/inference/stable/2023-05-15/inference.json"
  }
}

resource "azurerm_api_management_api_policy" "openai_inbound_policy" {
  api_management_name = azurerm_api_management_api.openai_api.api_management_name
  api_name            = azurerm_api_management_api.openai_api.name
  resource_group_name = azurerm_api_management_api.openai_api.resource_group_name

  xml_content = <<XML
<policies>
  <inbound>
    <base/>
    <set-variable name="backendId" value="@(new Random(context.RequestId.GetHashCode()).Next(1, ${length(azurerm_cognitive_account.openai) + 1}))" />
    <choose>
      <when condition="@(context.Variables.GetValueOrDefault<int>("backendId") == 1)">
        <set-backend-service backend-id="${azurerm_api_management_backend.openai_backends["OAI1"].name}"/>
      </when>
      <when condition="@(context.Variables.GetValueOrDefault<int>("backendId") == 2)">
        <set-backend-service backend-id="${azurerm_api_management_backend.openai_backends["OAI2"].name}"/>
      </when>
      <when condition="@(context.Variables.GetValueOrDefault<int>("backendId") == 3)">
        <set-backend-service backend-id="${azurerm_api_management_backend.openai_backends["OAI3"].name}"/>
      </when>
      <when condition="@(context.Variables.GetValueOrDefault<int>("backendId") == 4)">
        <set-backend-service backend-id="${azurerm_api_management_backend.openai_backends["OAI4"].name}"/>
      </when>
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

resource "azurerm_api_management_backend" "openai_backends" {
  for_each = azurerm_cognitive_account.openai

  api_management_name = azurerm_api_management.openai_apim.name
  name                = join("-", [local.resource_prefix, each.key, "apibackend"])
  protocol            = "http"
  resource_group_name = azurerm_api_management.openai_apim.resource_group_name
  url                 = join("", [each.value.endpoint, "openai"])

  credentials {
    header = {
      "api-key" = join(",", [
        "{{${azurerm_api_management_named_value.openai_primary_key[each.key].name}}}",
        "{{${azurerm_api_management_named_value.openai_secondary_key[each.key].name}}}"
      ])
    }
  }

  tls {
    validate_certificate_chain = true
    validate_certificate_name  = true
  }
}

resource "azurerm_api_management_named_value" "openai_primary_key" {
  for_each = azurerm_key_vault_secret.openai_primary_key

  api_management_name = azurerm_api_management.openai_apim.name
  display_name        = each.value.name
  name                = each.value.name
  resource_group_name = azurerm_api_management.openai_apim.resource_group_name
  secret              = true

  value_from_key_vault {
    secret_id = each.value.id
  }

  depends_on = [
    azurerm_role_assignment.openai_apim
  ]
}

resource "azurerm_api_management_named_value" "openai_secondary_key" {
  for_each = azurerm_key_vault_secret.openai_secondary_key

  api_management_name = azurerm_api_management.openai_apim.name
  display_name        = each.value.name
  name                = each.value.name
  resource_group_name = azurerm_api_management.openai_apim.resource_group_name
  secret              = true

  value_from_key_vault {
    secret_id = each.value.id
  }

  depends_on = [
    azurerm_role_assignment.openai_apim
  ]
}

resource "azurerm_cognitive_account" "openai" {
  for_each = local.openai

  custom_subdomain_name         = lower(join("-", [local.resource_prefix, each.key]))
  kind                          = "OpenAI"
  location                      = local.location
  name                          = join("-", [local.resource_prefix, each.key, "openai"])
  public_network_access_enabled = false
  resource_group_name           = azurerm_resource_group.rgs["OAI"].name
  sku_name                      = "S0"
  tags                          = local.tags
}

resource "azurerm_cognitive_deployment" "completions" {
  for_each = azurerm_cognitive_account.openai

  cognitive_account_id = each.value.id
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

resource "azurerm_key_vault_secret" "openai_primary_key" {
  for_each = azurerm_cognitive_account.openai

  key_vault_id = module.openai_keyvault.id
  name         = join("-", [local.resource_prefix, each.key, "primarykey"])
  value        = each.value.primary_access_key
}

resource "azurerm_key_vault_secret" "openai_secondary_key" {
  for_each = azurerm_cognitive_account.openai

  key_vault_id = module.openai_keyvault.id
  name         = join("-", [local.resource_prefix, each.key, "secondarykey"])
  value        = each.value.secondary_access_key
}

resource "azurerm_private_dns_a_record" "apim_azure_api" {
  name                = lower(azurerm_api_management.openai_apim.name)
  records             = azurerm_api_management.openai_apim.private_ip_addresses
  resource_group_name = local.private_dns_zones["azure-api.net"].resource_group_name
  ttl                 = 0
  zone_name           = local.private_dns_zones["azure-api.net"].name
}

resource "azurerm_private_dns_a_record" "apim_developer_azure_api" {
  name                = lower(azurerm_api_management.openai_apim.name)
  records             = azurerm_api_management.openai_apim.private_ip_addresses
  resource_group_name = local.private_dns_zones["developer.azure-api.net"].resource_group_name
  ttl                 = 0
  zone_name           = local.private_dns_zones["developer.azure-api.net"].name
}

resource "azurerm_private_dns_a_record" "apim_management_azure_api" {
  name                = lower(azurerm_api_management.openai_apim.name)
  records             = azurerm_api_management.openai_apim.private_ip_addresses
  resource_group_name = local.private_dns_zones["management.azure-api.net"].resource_group_name
  ttl                 = 0
  zone_name           = local.private_dns_zones["management.azure-api.net"].name
}

resource "azurerm_private_dns_a_record" "apim_portal_azure_api" {
  name                = lower(azurerm_api_management.openai_apim.name)
  records             = azurerm_api_management.openai_apim.private_ip_addresses
  resource_group_name = local.private_dns_zones["portal.azure-api.net"].resource_group_name
  ttl                 = 0
  zone_name           = local.private_dns_zones["portal.azure-api.net"].name
}

resource "azurerm_private_dns_a_record" "apim_scm_azure_api" {
  name                = lower(azurerm_api_management.openai_apim.name)
  records             = azurerm_api_management.openai_apim.private_ip_addresses
  resource_group_name = local.private_dns_zones["scm.azure-api.net"].resource_group_name
  ttl                 = 0
  zone_name           = local.private_dns_zones["scm.azure-api.net"].name
}

resource "azurerm_private_endpoint" "openai_ple" {
  for_each = azurerm_cognitive_account.openai

  location            = local.location
  name                = join("-", [local.resource_prefix, each.key, "openai", "ple"])
  resource_group_name = azurerm_resource_group.rgs["NET"].name
  subnet_id           = azurerm_subnet.subnets["FLLMOpenAI"].id

  private_service_connection {
    is_manual_connection           = false
    name                           = join("-", [local.resource_prefix, each.key, "openai", "psc"])
    private_connection_resource_id = each.value.id
    subresource_names              = ["account"]
  }

  private_dns_zone_group {
    name                 = join("-", [local.resource_prefix, each.key, "openai", "dzg"])
    private_dns_zone_ids = [local.private_dns_zones["privatelink.openai.azure.com"].id]
  }
}

resource "azurerm_public_ip" "openai_apim_mgmt_ip" {
  allocation_method   = "Static"
  domain_name_label   = "openai-apim-mgmt"
  location            = local.location
  name                = join("-", [local.resource_prefix, "OAI", "pip"])
  resource_group_name = azurerm_resource_group.rgs["NET"].name
  sku                 = "Standard"
}

resource "azurerm_role_assignment" "openai_apim" {
  principal_id         = azurerm_api_management.openai_apim.identity.0.principal_id
  role_definition_name = "Key Vault Secrets User"
  scope                = module.openai_keyvault.id
}
