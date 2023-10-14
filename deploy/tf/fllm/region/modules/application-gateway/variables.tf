variable "action_group_id" {
  description = "The ID of the action group to send alerts to."
  type        = string
}

variable "backend_pool_ip_addresses" {
  description = "The IP addresses that make up the backend pool for this application gateway."
  type        = list(string)
}

variable "hostname" {
  description = "The hostname to use for the application gateway."
  type        = string
}

variable "identity_id" {
  description = "The ID of the identity to use for the application gateway."
  type        = string
}

variable "key_vault_secret_id" {
  description = "The ID of the key vault secret to use for the application gateway certificate."
  type        = string
}

variable "log_analytics_workspace_id" {
  description = "The ID of the Log Analytics workspace to send diagnostics data to."
  type        = string
}

variable "public_dns_zone" {
  type = object({
    name                = string
    resource_group_name = string
  })
}

variable "resource_group" {
  description = "The resource group to deploy resources into"

  type = object({
    location = string
    name     = string
  })
}

variable "resource_prefix" {
  description = "The name prefix for the cosmosdb resources."
  type        = string
}

variable "subnet_id" {
  description = "The subnet ID to integrate with."
  type        = string
}

variable "tags" {
  description = "A map of tags for the resource."
  type        = map(string)
}