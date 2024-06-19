param keyvaultName string
param location string = resourceGroup().location
param name string
param sku string = 'S0'
param tags object = {}

var secretNames = [
  'foundationallm-apis-gatekeeper-azurecontentsafety-apikey'
]

resource contentSafety 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: name
  location: location
  sku: {
    name: sku
  }
  kind: 'ContentSafety'
  properties: {
    customSubDomainName: name
    publicNetworkAccess: 'Enabled'
  }
  tags: tags
}

resource keyvault 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
  name: keyvaultName
}

resource apiKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = [
  for secretName in secretNames: {
    name: secretName
    parent: keyvault
    tags: tags
    properties: {
      value: contentSafety.listKeys().key1
    }
  }
]

output endpoint string = contentSafety.properties.endpoint
output keySecretName string = apiKeySecret[0].name
output keySecretRef string = apiKeySecret[0].properties.secretUri
output name string = contentSafety.name
