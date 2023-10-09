variable "location" {
  type        = string
  description = "The location/region where the resource group is created"
}

variable "location_short" {
  type        = string
  description = "The location of the resource group"
}

variable "resource_groups" {
  type = map(object({
    tags = map(string)
  }))
}

variable "resource_prefix" {
  type        = string
  description = "The prefix used for all resources in this example"
}

variable "tags" {
  type        = map(string)
  description = "A mapping of tags to assign to the resource"
}

variable "vnet_address_space" {
  type        = string
  description = "Address space of the regional VNET"
}

variable "private_dns_zones" {
  type = map(object({
    id = string
    resource_group_name = string
  }))
}