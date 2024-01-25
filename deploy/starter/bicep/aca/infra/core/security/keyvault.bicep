metadata description = 'Creates an Azure Key Vault.'
param name string
param location string = resourceGroup().location
param tags object = {}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    createMode: 'default'
    enableRbacAuthorization: true
    enableSoftDelete: true
    enabledForDeployment: true
    sku: { family: 'A', name: 'standard' }
    tenantId: subscription().tenantId
  }
}

output endpoint string = keyVault.properties.vaultUri
output name string = keyVault.name
