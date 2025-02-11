/** Inputs **/
param actionGroupId string

param globalDnsResourceGroup string
param dnsSubscriptionId string = subscription().subscriptionId

param environmentName string
param location string
param logAnalyticsWorkspaceId string
param opsResourceGroupName string
param opsKeyVaultName string
param project string
param timestamp string = utcNow()
param vnetId string

param existingOpenAiInstance object

/** Locals **/
var deployments = filter(deploymentConfigurations, (d) => contains(d.locations, location))
var kvResourceSuffix = '${project}-${environmentName}-${location}-ops'
var resourceSuffix = '${project}-${environmentName}-${location}-${workload}'
var workload = 'oai'
var deployOpenAi = empty(existingOpenAiInstance.name)
var azureOpenAiEndpoint = deployOpenAi ? openai.outputs.endpoint : customerOpenAi.properties.endpoint
var azureOpenAiId = deployOpenAi ? openai.outputs.id : customerOpenAi.id
var azureOpenAi = deployOpenAi ? openAiInstance : existingOpenAiInstance
var azureOpenAiName = deployOpenAi ? openai.outputs.name : existingOpenAiInstance.name
var azureOpenAiRg = deployOpenAi ? resourceGroup().name : existingOpenAiInstance.resourceGroup
var azureOpenAiSubId = deployOpenAi ? subscription().subscriptionId : existingOpenAiInstance.subscriptionId
var azureOpenAiSub = deployOpenAi ? subscription() : subscription(existingOpenAiInstance.subscriptionId)
var openAiInstance = {
  name: azureOpenAiName
  resourceGroup: azureOpenAiRg
  subscriptionId: azureOpenAiSubId
}

var deploymentConfigurations = loadJsonContent('../../common/config/openAiDeploymentConfig.json')

var tags = {
  'azd-env-name': environmentName
  'iac-type': 'bicep'
  'project-name': project
  Purpose: 'OpenAI'
}

/** Nested Modules **/
@description('Read DNS Zones')
module globalDnsZones 'modules/utility/globalDnsZoneData.bicep' = {
  name: 'glbDnsZones-${timestamp}'
  scope: resourceGroup(dnsSubscriptionId, globalDnsResourceGroup)
  params: {
    locations: [location]
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
    privateDnsZones: filter(globalDnsZones.outputs.ids, (zone) => zone.key == 'cognitiveservices')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/openai'
    tags: tags
  }
}

@description('OpenAI')
module openai './modules/openai.bicep' = if (deployOpenAi) {
  name: 'openai-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    deployments: deployments
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    privateDnsZones: filter(globalDnsZones.outputs.ids, (zone) => zone.key == 'openai')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/openai'
    tags: tags
  }
}

module openAiSecrets './modules/openai-secrets.bicep' = {
  name: 'openaiSecrets-${timestamp}'

  params: {
    keyvaultName: opsKeyVaultName
    openAiInstance: azureOpenAi
    tags: tags
  }

  scope: resourceGroup(opsResourceGroupName)
  dependsOn: deployOpenAi ? [ openai ] : []
}

resource customerOpenAiResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing =
  if (!deployOpenAi) {
    scope: azureOpenAiSub
    name: existingOpenAiInstance.resourceGroup
  }

resource customerOpenAi 'Microsoft.CognitiveServices/accounts@2023-05-01' existing =
  if (!deployOpenAi) {
    name: existingOpenAiInstance.name
    scope: customerOpenAiResourceGroup
  }

output azureContentSafetyEndpoint string = contentSafety.outputs.endpoint

output azureOpenAiEndpoint string = azureOpenAiEndpoint
output azureOpenAiId string = azureOpenAiId
output azureOpenAiResourceGroup string = openAiInstance.resourceGroup
output azureOpenAiName string = openAiInstance.name
  