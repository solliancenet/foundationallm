variable "location" {
  type        = string
  description = "The location/region where the resource group is created"
}

variable "location_short" {
  default     = "GLB"
  type        = string
  description = "The location of the resource group"
}

variable "public_domain" {
  type = string
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