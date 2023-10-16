locals {
  alert = {
    backendhealth = {
      aggregation = "Average"
      description = "Backend health is less than 1 for 5 minutes"
      frequency   = "PT1M"
      metric_name = "HeatlhyHostCount"
      operator    = "LessThan"
      severity    = 0
      threshold   = 1
      window_size = "PT5M"
    }
    dataprocessed = {
      aggregation = "Average"
      description = "Data processed is greater than 100000 for 5 minutes"
      frequency   = "PT1M"
      metric_name = "BytesSent"
      operator    = "GreaterThan"
      severity    = 2
      threshold   = 100000
      window_size = "PT5M"
    }
    failedrequests = {
      aggregation = "Average"
      description = "Failed requests are greater than 10 for 5 minutes"
      frequency   = "PT1M"
      metric_name = "FailedRequests"
      operator    = "GreaterThan"
      severity    = 1
      threshold   = 10
      window_size = "PT5M"
    }
    requests = {
      aggregation = "Average"
      description = "Requests are greater than 1000 for 5 minutes"
      frequency   = "PT1M"
      metric_name = "TotalRequests"
      operator    = "GreaterThan"
      severity    = 2
      threshold   = 1000
      window_size = "PT5M"
    }
  }
}

resource "azurerm_application_gateway" "main" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-agw"
  resource_group_name = var.resource_group.name
  tags                = var.tags

  autoscale_configuration {
    max_capacity = 3
    min_capacity = 0
  }

  backend_address_pool {
    ip_addresses = var.backend_pool_ip_addresses
    name         = "default"
  }

  backend_http_settings {
    cookie_based_affinity = "Disabled"
    name                  = "http"
    path                  = "/"
    port                  = 80
    protocol              = "Http"
    request_timeout       = 60
  }

  backend_http_settings {
    cookie_based_affinity = "Disabled"
    name                  = "https"
    path                  = "/"
    port                  = 443
    probe_name            = "https"
    protocol              = "Https"
    request_timeout       = 180
  }

  frontend_ip_configuration {
    name                 = "default"
    public_ip_address_id = azurerm_public_ip.pip.id
  }

  frontend_port {
    name = "https"
    port = 443
  }

  frontend_port {
    name = "http"
    port = 80
  }

  gateway_ip_configuration {
    name      = "default"
    subnet_id = var.subnet_id
  }

  http_listener {
    frontend_ip_configuration_name = "default"
    frontend_port_name             = "http"
    host_name                      = var.hostname
    name                           = "http"
    protocol                       = "Http"
  }

  http_listener {
    frontend_ip_configuration_name = "default"
    frontend_port_name             = "https"
    host_name                      = var.hostname
    name                           = "https"
    protocol                       = "Https"
    require_sni                    = true
    ssl_certificate_name           = "default"
  }

  identity {
    identity_ids = [var.identity_id]
    type         = "UserAssigned"
  }

  probe {
    host                = var.hostname
    interval            = 30
    name                = "https"
    path                = "/"
    protocol            = "Https"
    timeout             = 1
    unhealthy_threshold = 3
  }

  redirect_configuration {
    name                 = "http-to-https"
    redirect_type        = "Permanent"
    target_listener_name = "https"
  }

  request_routing_rule {
    backend_address_pool_name  = "default"
    backend_http_settings_name = "https"
    http_listener_name         = "https"
    name                       = "https"
    priority                   = 100
    rule_type                  = "Basic"
  }

  request_routing_rule {
    http_listener_name          = "http"
    name                        = "http"
    priority                    = 200
    redirect_configuration_name = "http-to-https"
    rule_type                   = "Basic"
  }

  sku {
    name = "WAF_v2"
    tier = "WAF_v2"
  }

  ssl_certificate {
    key_vault_secret_id = var.key_vault_secret_id
    name                = "default"
  }

  ssl_policy {
    policy_name = "AppGwSslPolicy20170401S"
    policy_type = "Predefined"
  }

  waf_configuration {
    enabled          = true
    firewall_mode    = "Prevention"
    rule_set_type    = "OWASP"
    rule_set_version = "3.1"
  }
}

resource "azurerm_dns_a_record" "a" {
  name                = "www"
  zone_name           = var.public_dns_zone.name
  resource_group_name = var.public_dns_zone.resource_group_name
  ttl                 = 300
  records             = [azurerm_public_ip.pip.ip_address]
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-agw-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_application_gateway.main.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.Network/ApplicationGateways"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}

resource "azurerm_public_ip" "pip" {
  allocation_method   = "Static"
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-pip"
  resource_group_name = var.resource_group.name
  sku                 = "Standard"
  tags                = var.tags
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    agw = {
      id = azurerm_application_gateway.main.id
    }
    pip = {
      id = azurerm_public_ip.pip.id
    }
  }
}
