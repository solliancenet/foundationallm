targetScope = 'subscription'

// Inputs
param createDate string = utcNow('u')
param timestamp string = utcNow()

@minLength(1)
@maxLength(64)
param environmentName string

@minLength(1)
param location string

// Locals
var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))

var tags = {
  'azd-env-name': environmentName
  'compute-type': 'container-app'
  'create-date': createDate
}

// Resources
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${abbrs.resourcesResourceGroups}ext-${environmentName}'
  location: location
  tags: tags
}

// Nested Deployments
module contentSafety 'core/ai/cognitiveservices.bicep' = {
  name: 'contentSafety-${timestamp}'
  scope: rg
  params: {
    kind: 'ContentSafety'
    location: location
    name: '${abbrs.cognitiveServicesAccounts}cs-${resourceToken}'
    tags: tags
  }
}

module hub 'core/ai/hub.bicep' = {
  name: 'hub-${timestamp}'
  scope: rg
  params: {
    aiSearchConnectionName: '${hubDependencies.outputs.searchServiceName}-connection'
    aiSearchName: hubDependencies.outputs.searchServiceName
    contentSafetyName: contentSafety.outputs.name
    keyVaultId: hubDependencies.outputs.keyVaultId
    name: 'aihub-${resourceToken}'
    openAiName: hubDependencies.outputs.openAiName
    storageAccountId: hubDependencies.outputs.storageAccountId
    tags: tags
  }
}

module hubDependencies 'core/ai/hub-dependencies.bicep' = {
  name: 'hubDependencies-${timestamp}'
  scope: rg
  params: {
    keyVaultName: '${abbrs.keyVaultVaults}${resourceToken}'
    location: location
    openAiName: '${abbrs.cognitiveServicesAccounts}${abbrs.openai}${resourceToken}'
    searchServiceName: '${abbrs.searchSearchServices}${resourceToken}'
    storageAccountName: '${abbrs.storageStorageAccounts}${resourceToken}'
    tags: tags
  }
}

module project 'core/ai/project.bicep' = {
  name: 'project-${timestamp}'
  scope: rg
  params: {
    hubName: hub.outputs.name
    keyVaultName: hubDependencies.outputs.keyVaultName
    location: location
    name: 'aiproject-${resourceToken}'
    resourceToken: resourceToken
    tags: tags
  }
}

// Outputs
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output MISTRAL_DEPLOYMENT_NAME string = project.outputs.mistralDeploymentName
output MISTRAL_ENDPOINT_NAME string = project.outputs.mistralEndpointName
output PHI_DEPLOYMENT_NAME string = project.outputs.phiDeploymentName
output PHI_ENDPOINT_NAME string = project.outputs.phiEndpointName
output PROJECT_WORKSPACE_NAME string = project.outputs.name
output RESOURCE_GROUP_NAME string = rg.name
