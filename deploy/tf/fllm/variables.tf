variable "environment" {
  type    = string
  default = "DEMO"
}

variable "global_location" {
  type    = string
  default = "East US"
}

variable "project_id" {
  type    = string
  default = "FLLM"
}

variable "tags" {
  type    = map(string)
  default = {}
}