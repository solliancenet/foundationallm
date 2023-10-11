variable "action_group_id" {
  description = "The ID of the action group to send alerts to."
  type        = string
}

variable "log_analytics_workspace" {
  description = "The Log Analytics Workspace for diagnostics."
  sensitive   = true

  type = object({
    workspace_id       = string
    primary_shared_key = string
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
  description = "The name prefix for the TFC Agent"
  type        = string
}

variable "subnet_id" {
  description = "The subnet ID to integrate with."
  type        = string
}

variable "tags" {
  description = "A map of tags for the resource."
  type        = map(string)
}

variable "tfc_agent_token" {
  description = "The token used by the agent to authenticate with Terraform Cloud."
  sensitive   = true
  type        = string
}