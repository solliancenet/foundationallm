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