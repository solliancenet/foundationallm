variable "action_group_id" {
  description = "The ID of the action group to send alerts to."
  type        = string
}

variable "encryption_keyvault_id" {
  description = "The ID of the KeyVault that will handle encryption of configurations."
  type        = string
}

variable "log_analytics_workspace_id" {
  description = "The ID of the Log Analytics workspace to send diagnostics data to."
  type        = string
}

variable "private_endpoint" {
  description = "The private endpoint configuration."
  type = object({
    subnet_id            = string
    private_dns_zone_ids = list(string)
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
  description = "The name prefix for the Log Analytics workspace."
  type        = string
}

variable "tags" {
  description = "A map of tags for the resource."
  type        = map(string)
}