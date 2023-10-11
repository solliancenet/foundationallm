variable "log_analytics_workspace_id" {
  description = "The workspace to write logs into."
  type        = string
}

variable "monitored_services" {
  description = "A map of service names to thier resource ids that should be configured to send diagnostics to log analytics."
  type = map(object({
    id      = string
    table   = optional(string)
    include = optional(list(string), [])
  }))
}