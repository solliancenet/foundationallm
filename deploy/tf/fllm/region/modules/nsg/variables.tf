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

variable "subnet_id" {
  description = "The subnet id to associate the NSG with"
  type        = string
}

variable "tags" {
  description = "The tags to use on each resource"
  type        = map(string)
}
