variable "action_group_id" {
  description = "The ID of the action group to send alerts to."
  type        = string
}

variable "instance_count" {
  description = "The number of OpenAI instances to load balance across"
  type        = number
  default     = 4
}

variable "log_analytics_workspace_id" {
  description = "The ID of the Log Analytics workspace to send diagnostics data to."
  type        = string
}

variable "private_endpoint" {
  description = "The private endpoint configuration for KeyVault."
  type = object({
    subnet_id = string
    apim_private_dns_zones = list(object({
      name                = string
      resource_group_name = string
    }))
    kv_private_dns_zone_ids     = list(string)
    openai_private_dns_zone_ids = list(string)
  })
}

variable "publisher" {
  description = "The API publisher details"
  type = object({
    email = string
    name  = string
  })
  default = {
    email = "info@solliance.net"
    name  = "FoundationaLLM"
  }
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