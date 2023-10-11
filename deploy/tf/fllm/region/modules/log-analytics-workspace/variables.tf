variable "action_group_id" {
  description = "The ID of the action group to send alerts to."
  type        = string
}

variable "azure_monitor_private_link_scope" {
  description = "The Azure Monitor Private Link Scope."
  type = object({
    name                = string
    resource_group_name = string
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

variable "resource_group" {
  description = "The resource group to deploy resources into"

  type = object({
    location = string
    name     = string
  })
}

variable "solutions" {
  description = "The Log Analytics solutions to add to the workspace."
  default     = {}

  type = map(object({
    publisher = string
    product   = string
  }))
}