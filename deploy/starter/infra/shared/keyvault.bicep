param name string
param location string = resourceGroup().location
param tags object = {}
param secrets array = []

@description('Service principal that should be granted read access to the KeyVault. If unset, no service principal is granted access by default')
param principalId string = ''

var defaultAccessPolicies = !empty(principalId) ? [
  {
    objectId: principalId
    permissions: { secrets: [ 'get', 'list' ] }
    tenantId: subscription().tenantId
  }
] : []

resource adminRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(principalId, resourceGroup().id, 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7')
  properties: {
    principalId: principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefiniitions', 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7')
    principalType: 'servicePrincipal'
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    tenantId: subscription().tenantId
    sku: { family: 'A', name: 'standard' }
    enabledForTemplateDeployment: true
    enableRbacAuthorization: true
    accessPolicies: union(defaultAccessPolicies, [
      // define access policies here
    ])
  }
}

resource kvSecrets 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = [
  for secret in secrets: {
    name: secret.name
    parent: keyVault
    tags: tags
    properties: {
      value: secret.value
    }
  }
]

output endpoint string = keyVault.properties.vaultUri
output name string = keyVault.name
