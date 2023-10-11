variable "resource_group" {
  description = "The resource group to deploy resources into"

  type = object({
    location = string
    name     = string
  })
}

variable "resource_prefix" {
  description = "The name prefix for the NSG"
  type        = string
}

variable "rules_inbound" {
  description = "The inbound rules for the NSG."
  type = map(object({
    access                       = string
    destination_address_prefix   = optional(string)
    destination_address_prefixes = optional(list(string))
    destination_port_range       = string
    priority                     = number
    protocol                     = string
    source_address_prefix        = optional(string)
    source_address_prefixes      = optional(list(string))
    source_port_range            = string
  }))
}

variable "rules_outbound" {
  description = "The outbound rules for the NSG."
  type = map(object({
    access                       = string
    destination_address_prefix   = optional(string)
    destination_address_prefixes = optional(list(string))
    destination_port_range       = string
    priority                     = number
    protocol                     = string
    source_address_prefix        = optional(string)
    source_address_prefixes      = optional(list(string))
    source_port_range            = string
  }))
}

variable "subnet_id" {
  description = "The subnet id to associate the NSG with"
  type        = string
}

variable "tags" {
  description = "The tags to use on each resource"
  type        = map(string)
}
