variable "azp_url" {
  type = string
}

variable "azp_token" {
  type = string
}

variable "azp_agent_name" {
  type = string
}

variable "environment" {
  description = "The environment name"
  type        = string
  default     = "DEMO"
}

variable "global_location" {
  description = "The global location"
  type        = string
  default     = "East US"
}

variable "project_id" {
  description = "The project id"
  type        = string
  default     = "FLLM"
}

variable "public_domain" {
  description = "Public DNS domain"
  type        = string
  default     = "internal.foundationallm.ai"
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

variable "tags" {
  description = "The tags to use on each resource"
  type        = map(string)
  default     = {}
}

variable "tfc_agent_token" {
  description = "The token used by the agent to authenticate with Terraform Cloud."
  sensitive   = true
  type        = string
}