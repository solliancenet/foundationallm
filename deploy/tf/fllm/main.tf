locals {
  environment     = var.environment
  global_location = var.global_location
  project_id      = var.project_id
  resource_prefix = join("-", [local.project_id, local.environment])

  global_resource_groups = {
  }

  regional_resource_groups = {
    "NET" = {
      tags = {}
    }
  }

  regions = {
    "East US" = {
      location_short = "EUS"
    }
  }

  tags = merge(var.tags, {
    "Project"     = local.project_id
    "Environment" = local.environment
  })
}
