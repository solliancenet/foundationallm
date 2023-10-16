variable "location" {
  description = "The location/region where the resource group is created"
  type        = string
}

variable "location_short" {
  description = "The location of the resource group"
  type        = string
}

variable "private_dns_zones" {
  type = map(object({
    id                  = string
    name                = string
    resource_group_name = string
  }))
}

variable "public_dns_zone" {
  type = object({
    id                  = string
    name                = string
    resource_group_name = string
  })
}

variable "resource_groups" {
  type = map(object({
    tags = map(string)
  }))
}

variable "resource_prefix" {
  description = "The prefix used for all resources in this example"
  type        = string
}

variable "sql_admin_ad_group" {
  description = "SQL Admin AD group"
  type = object({
    name      = string
    object_id = string
  })
  default = {
    name      = "FoundationaLLM SQL Admins"
    object_id = "73d59f98-857b-45e7-950b-5ee30d289bc8"
  }
}

variable "vnet_address_space" {
  description = "Address space of the regional VNET"
  type        = string
}

variable "tfc_agent_token" {
  description = "The token used by the agent to authenticate with Terraform Cloud."
  sensitive   = true
  type        = string
}

variable "tags" {
  description = "A mapping of tags to assign to the resource"
  type        = map(string)
}