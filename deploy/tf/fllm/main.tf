locals {
  environment     = var.environment
  global_location = var.global_location
  project_id      = var.project_id
  resource_prefix = join("-", [local.project_id, local.environment])

  global_resource_groups = {
    "DNS" = {
      tags = {
        "Purpose" = "Networking"
      }
    }
  }

  regional_resource_groups = {
    "JBX" = {
      tags = {
        "Purpose" = "DevOps"
      }
    }
    "NET" = {
      tags = {
        "Purpose" = "Networking"
      }
    }
    "OAI" = {
      tags = {
        "Purpose" = "OpenAI"
      }
    }
  }

  regions = {
    "East US" = {
      location_short     = "EUS"
      vnet_address_space = "10.0.0.0/16"
    }
  }

  tags = merge(var.tags, {
    "Project"     = local.project_id
    "Environment" = local.environment
  })
}
