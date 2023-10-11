# Foundational LLM Standard Deployment

<!-- BEGIN_TF_DOCS -->
## Requirements

The following requirements are needed by this module:

- <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) (3.65.0)

## Providers

No providers.

## Modules

The following Modules are called:

### <a name="module_global"></a> [global](#module\_global)

Source: ./global

Version:

### <a name="module_regions"></a> [regions](#module\_regions)

Source: ./region

Version:

## Resources

No resources.

## Required Inputs

No required inputs.

## Optional Inputs

The following input variables are optional (have default values):

### <a name="input_environment"></a> [environment](#input\_environment)

Description: The environment name

Type: `string`

Default: `"DEMO"`

### <a name="input_global_location"></a> [global\_location](#input\_global\_location)

Description: The global location

Type: `string`

Default: `"East US"`

### <a name="input_project_id"></a> [project\_id](#input\_project\_id)

Description: The project id

Type: `string`

Default: `"FLLM"`

### <a name="input_tags"></a> [tags](#input\_tags)

Description: The tags to use on each resource

Type: `map(string)`

Default: `{}`

## Outputs

No outputs.
<!-- END_TF_DOCS -->