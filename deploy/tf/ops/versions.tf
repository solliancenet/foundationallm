terraform {
    required_providers {
        azurerm = {
            source = "hashicorp/azurerm"
            version = "3.65.0"
        }
    }

    cloud {
        organization = "FoundationaLLM"
        workspaces {
            name = "FoundationaLLM-OPS"
        }
    }
}

provider "azurerm" {
    features {}
}