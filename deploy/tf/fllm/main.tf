locals {
  environment     = var.environment
  global_location = var.global_location
  project_id      = var.project_id
  public_domain   = var.public_domain
  resource_prefix = join("-", [local.project_id, local.environment])

  global_resource_groups = {
    "DNS" = {
      tags = {
        "Purpose" = "Networking"
      }
    }
  }

  regional_resource_groups = {
    "AppGateway" = {
      tags = {
        "Purpose" = "Networking"
      }
    }
    "Data" = {
      tags = {
        "Purpose" = "Storage"
      }
    }
    "FLLMStorage" = {
      tags = {
        Purpose = "Storage"
      }
    }
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
    "OPS" = {
      tags = {
        "Purpose" = "DevOps"
      }
    }
    "VEC" = {
      tags = {
        "Purpose" = "Vectorization"
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

module "global" {
  source = "./global"

  location        = local.global_location
  public_domain   = local.public_domain
  resource_groups = local.global_resource_groups
  resource_prefix = local.resource_prefix
  tags            = local.tags
}

module "regions" {
  source   = "./region"
  for_each = local.regions

  location           = each.key
  location_short     = each.value.location_short
  private_dns_zones  = module.global.private_dns_zones
  public_dns_zone    = module.global.public_dns_zone
  resource_groups    = local.regional_resource_groups
  resource_prefix    = local.resource_prefix
  tags               = local.tags
  tfc_agent_token    = var.tfc_agent_token
  vnet_address_space = each.value.vnet_address_space
}