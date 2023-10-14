variable "administrator_email" {
  description = "The email address of the certificate administrator."
  type        = string
}

variable "domain" {
  description = "The domain name to request a certificate for."
  type        = string
}

variable "key_vault_id" {
  description = "The ID of the key vault to store the certificate in."
  type        = string
}

variable "public_dns_zone" {
  type = object({
    name                = string
    resource_group_name = string
  })
}