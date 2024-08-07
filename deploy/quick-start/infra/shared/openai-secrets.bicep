param keyvaultName string
param openAiInstance object
param tags object = {}

resource apiKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  name: '${keyvaultName}/foundationallm-apiendpoints-azureopenai-apikey'
  tags: tags

  properties: {
    value: openAi.listKeys().key1
  }
}

resource openAiResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = {
  scope: subscription(openAiInstance.subscriptionId)
  name: openAiInstance.resourceGroup
}

resource openAi 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = {
  name: openAiInstance.name
  scope: openAiResourceGroup
}

output keySecretName string = apiKeySecret.name
output keySecretRef string = apiKeySecret.properties.secretUri
