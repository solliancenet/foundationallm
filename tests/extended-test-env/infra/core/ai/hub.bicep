param aiSearchConnectionName string
param aiSearchName string
param aiStudioSkuName string = 'Basic'
param contentSafetyName string
param displayName string = name
param keyVaultId string
param location string = resourceGroup().location
param name string
param openAiName string
param storageAccountId string
param tags object = {}

@allowed(['Basic', 'Free', 'Premium', 'Standard'])
param aiStudioSkuTier string = 'Basic'

@allowed(['Enabled','Disabled'])
param publicNetworkAccess string = 'Enabled'

resource hub 'Microsoft.MachineLearningServices/workspaces@2024-01-01-preview' = {
  name: name
  location: location
  tags: tags
  kind: 'Hub'

  sku: {
    name: aiStudioSkuName
    tier: aiStudioSkuTier
  }

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    friendlyName: displayName
    hbiWorkspace: false
    keyVault: keyVaultId
    publicNetworkAccess: publicNetworkAccess
    storageAccount: storageAccountId
    v1LegacyMode: false

    managedNetwork: {
      isolationMode: 'Disabled'
    }
  }

  resource contentSafetyConnection 'connections' ={
    name: '${contentSafetyName}-connection'
    properties:{
      authType:'ApiKey'
      category: 'CognitiveService'
      target:'https://${contentSafety.name}.cognitiveservices.azure.com/'
      isSharedToAll: true
      metadata:{
        ApiType: 'azure'
        ApiVersion: '2023-10-01'
        Kind: 'Content Safety'
        ResourceId: contentSafety.id
      }
      credentials:{
        key: contentSafety.listKeys().key1
      }
    }
  }

  resource aiServicesConnection 'connections' = {
    name: 'aiServices-${openAiName}-connection'
    properties: {
      authType: 'ApiKey'
      category: 'AIServices'
      isSharedToAll: true
      target: 'https://${openAiName}.openai.azure.com/'
      metadata: {
        ApiVersion: '2023-07-01-preview'
        ApiType: 'Azure'
        ResourceId: openAi.id
      }
      credentials:{
        key: openAi.listKeys().key1
      }
    }
  }

  resource searchConnection 'connections' =
    if (!empty(aiSearchName)) {
      name: aiSearchConnectionName
      properties: {
        category: 'CognitiveSearch'
        authType: 'ApiKey'
        isSharedToAll: true
        target: 'https://${search.name}.search.windows.net/'
        credentials: {
          key: !empty(aiSearchName) ? search.listAdminKeys().primaryKey : ''
        }
      }
    }
}

resource contentSafety 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = {
  name: contentSafetyName
}

resource openAi 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = {
  name: openAiName
}

resource search 'Microsoft.Search/searchServices@2021-04-01-preview' existing =
  if (!empty(aiSearchName)) {
    name: aiSearchName
  }

output name string = hub.name
output id string = hub.id
output principalId string = hub.identity.principalId
