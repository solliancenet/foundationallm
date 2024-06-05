/** Inputs **/
param actionGroupId string
param administratorObjectId string
param dnsResourceGroupName string
param environmentName string
param instanceCount int = 1
param location string
param logAnalyticsWorkspaceId string
param opsResourceGroupName string
param project string
param timestamp string = utcNow()
param vnetId string

/** Locals **/
var deployments = filter(deploymentConfigurations, (d) => contains(d.locations, location))
var kvResourceSuffix = '${project}-${environmentName}-${location}-ops'
var resourceSuffix = '${project}-${environmentName}-${location}-${workload}'
var workload = 'oai'

var deploymentConfigurations = [
  {
    name: 'completions'
    locations: [
      'eastus'
      'eastus2'
      'japaneast'
      'northcentralus'
      'switzerlandnorth'
    ]
    raiPolicyName: ''
    model: {
      format: 'OpenAI'
      name: 'gpt-35-turbo'
      version: '0613'
    }
    sku: {
      capacity: 60
      name: 'Standard'
    }
  }
  {
    name: 'completions'
    locations: [
      'austrailiaeast'
      'canadaeast'
      'francecentral'
      'southindia'
      'swedencentral'
      'uksouth'
      'westus'
    ]
    raiPolicyName: ''
    model: {
      format: 'OpenAI'
      name: 'gpt-35-turbo'
      version: '1106'
    }
    sku: {
      capacity: 60
      name: 'Standard'
    }
  }
  {
    name: 'completions4'
    locations: [
      'austrailiaeast'
      'canadaeast'
      'eastus2'
      'francecentral'
      'norwayeast'
      'southindia'
      'swedencentral'
      'uksouth'
      'westus'
    ]
    raiPolicyName: ''
    model: {
      format: 'OpenAI'
      name: 'gpt-4'
      version: '1106-Preview'
    }
    sku: {
      capacity: 40
      name: 'Standard'
    }
  }
  {
    name: 'completions4o'
    locations: [
      'eastus'
      'eastus2'
      'northcentralus'
      'southcentralus'
      'southindia'
      'westus'
      'westus3'
    ]
    raiPolicyName: ''
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
      version: '2024-05-13'
    }
    sku: {
      capacity: 40
      name: 'Standard'
    }
  }
  {
    name: 'embeddings'
    locations: [
      'austrailiaeast'
      'canadaeast'
      'eastus'
      'eastus2'
      'francecentral'
      'japaneast'
      'northcentralus'
      'norwayeast'
      'southcentralus'
      'swedencentral'
      'switzerlandnorth'
      'uksouth'
      'westeurope'
      'westus'
    ]
    raiPolicyName: 'Microsoft.Default'
    model: {
      format: 'OpenAI'
      name: 'text-embedding-ada-002'
      version: '2'
    }
    sku: {
      capacity: 60
      name: 'Standard'
    }
  }
  {
    name: 'embeddings-3-large'
    locations: [
      'canadaeast'
      'eastus'
      'eastus2'
    ]
    raiPolicyName: 'Microsoft.Default'
    model: {
      format: 'OpenAI'
      name: 'text-embedding-3-large'
      version: ''
    }
    sku: {
      capacity: 60
      name: 'Standard'
    }
  }
  {
    name: 'embeddings-3-small'
    locations: [
      'canadaeast'
      'eastus'
      'eastus2'
    ]
    raiPolicyName: 'Microsoft.Default'
    model: {
      format: 'OpenAI'
      name: 'text-embedding-3-small'
      version: ''
    }
    sku: {
      capacity: 60
      name: 'Standard'
    }
  }
]

var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'OpenAI'
}

/** Nested Modules **/
@description('Read DNS Zones')
module dnsZones 'modules/utility/dnsZoneData.bicep' = {
  name: 'dnsZones-${timestamp}'
  scope: resourceGroup(dnsResourceGroupName)
  params: {
    location: location
  }
}

@description('Content Safety')
module contentSafety 'modules/contentSaftey.bicep' = {
  name: 'contentSafety-${timestamp}'
  params: {
    kvResourceSuffix: kvResourceSuffix
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    opsResourceGroupName: opsResourceGroupName
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => zone.key == 'cognitiveservices')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/FLLMOpenAI'
    tags: tags
  }
  dependsOn: [ keyVault ]
}

@description('Key Vault')
module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVault-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    administratorObjectId: administratorObjectId
    allowAzureServices: false
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => zone.key == 'vault')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/FLLMOpenAI'
    tags: tags
  }
}

@description('OpenAI')
module openai './modules/openai.bicep' = [for x in range(0, instanceCount): {
  name: 'openai-${x}-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    deployments: deployments
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    opsKvResourceSuffix: kvResourceSuffix
    opsResourceGroupName: opsResourceGroupName
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => zone.key == 'openai')
    resourceSuffix: '${resourceSuffix}-${x}'
    subnetId: '${vnetId}/subnets/FLLMOpenAI'
    tags: tags
    keyVaultName: keyVault.outputs.name
  }
  dependsOn: [ keyVault ]
}]
