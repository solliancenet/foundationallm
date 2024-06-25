param keyVaultName string
param location string = resourceGroup().location
param openAiModelDeployments array = []
param openAiName string
param searchServiceName string
param storageAccountName string
param tags object = {}
param timestamp string = utcNow()

module keyVault '../security/keyvault.bicep' = {
  name: 'keyvault-${timestamp}'
  params: {
    location: location
    tags: tags
    name: keyVaultName
  }
}

module storageAccount '../storage/storage-account.bicep' = {
  name: 'storageAccount-${timestamp}'
  params: {
    location: location
    tags: tags
    name: storageAccountName
    containers: [
      {
        name: 'default'
      }
    ]
    files: [
      {
        name: 'default'
      }
    ]
    queues: [
      {
        name: 'default'
      }
    ]
    tables: [
      {
        name: 'default'
      }
    ]
    corsRules: [
      {
        allowedOrigins: [
          'https://mlworkspace.azure.ai'
          'https://ml.azure.com'
          'https://*.ml.azure.com'
          'https://ai.azure.com'
          'https://*.ai.azure.com'
          'https://mlworkspacecanary.azure.ai'
          'https://mlworkspace.azureml-test.net'
        ]
        allowedMethods: [
          'GET'
          'HEAD'
          'POST'
          'PUT'
          'DELETE'
          'OPTIONS'
          'PATCH'
        ]
        maxAgeInSeconds: 1800
        exposedHeaders: [
          '*'
        ]
        allowedHeaders: [
          '*'
        ]
      }
    ]
    deleteRetentionPolicy: {
      allowPermanentDelete: false
      enabled: false
    }
    shareDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

module cognitiveServices '../ai/cognitiveservices.bicep' = {
  name: 'cognitiveServices-${timestamp}'
  params: {
    location: location
    tags: tags
    name: openAiName
    kind: 'AIServices'
    deployments: openAiModelDeployments
  }
}

module searchService '../search/search-services.bicep' =
  if (!empty(searchServiceName)) {
    name: 'searchService-${timestamp}'
    params: {
      location: location
      tags: tags
      name: searchServiceName
    }
  }

output keyVaultEndpoint string = keyVault.outputs.endpoint
output keyVaultId string = keyVault.outputs.id
output keyVaultName string = keyVault.outputs.name
output openAiEndpoint string = cognitiveServices.outputs.endpoints['OpenAI Language Model Instance API']
output openAiId string = cognitiveServices.outputs.id
output openAiName string = cognitiveServices.outputs.name
output searchServiceEndpoint string = !empty(searchServiceName) ? searchService.outputs.endpoint : ''
output searchServiceId string = !empty(searchServiceName) ? searchService.outputs.id : ''
output searchServiceName string = !empty(searchServiceName) ? searchService.outputs.name : ''
output storageAccountId string = storageAccount.outputs.id
output storageAccountName string = storageAccount.outputs.name
