param adminGroupObjectId string
param containers array = [
  'role-assignments'
  'policy-assignments'
  'secret-keys'
]

param location string = resourceGroup().location
param name string
param tags object = {}

resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: name
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  tags: tags

  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    isHnsEnabled: true
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource contributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storage
  name: guid(subscription().id, resourceGroup().id, adminGroupObjectId, 'authAdminRole')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c')
    principalType: 'Group'
    principalId: adminGroupObjectId
  }
}

resource blobContributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storage
  name: guid(subscription().id, resourceGroup().id, adminGroupObjectId, 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalType: 'Group'
    principalId: adminGroupObjectId
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storage
  name: 'default'

  properties: {
    containerDeleteRetentionPolicy: {
      days: 30
      enabled: true
    }

    deleteRetentionPolicy: {
      allowPermanentDelete: false
      days: 30
      enabled: true
    }
  }
}

resource blobContainers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = [
for container in containers: {
    parent: blobService
    name: container
  }
]

output name string = storage.name
