terraform {
  required_version = "~> 1.6"

  required_providers {
    acme = {
      source  = "vancluever/acme"
      version = "~> 2.0"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.65"
    }
    tfe = {
      source  = "hashicorp/tfe"
      version = "~> 0.49"
    }
  }

  cloud {
    organization = "FoundationaLLM"
    workspaces {
      name = "FoundationaLLM-Platform"
    }
  }
}

