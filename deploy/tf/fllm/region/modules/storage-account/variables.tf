variable "action_group_id" {
  description = "The ID of the action group to send alerts to."
  type        = string
}

variable "containers" {
  default     = []
  description = "When provided the module will create private blob containers for each item in the list."
  type        = list(string)
}

variable "log_analytics_workspace_id" {
  description = "The ID of the Log Analytics workspace to send diagnostics data to."
  type        = string
}

variable "private_endpoint" {
  description = "The private endpoint configuration."
  type = object({
    subnet_id = string
    private_dns_zone_ids = object({
      blob  = list(string)
      dfs   = list(string)
      file  = list(string)
      queue = list(string)
      table = list(string)
      web   = list(string)
    })
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

variable "shares" {
  default     = {}
  description = "When provided the module will create file shares for each item in the list with optional quota."
  type = map(object({
    quota = optional(number)
  }))
}

variable "tags" {
  description = "A map of tags for the resource."
  type        = map(string)
}