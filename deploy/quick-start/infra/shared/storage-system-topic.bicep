param name string
param destinationTopicName string
param eventGridName string
param location string = resourceGroup().location
param tags object = {}
param storageAccountName string

resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

resource eventGridNamespace 'Microsoft.EventGrid/namespaces@2023-12-15-preview' existing = {
  name: eventGridName
}

resource destinationTopic 'Microsoft.EventGrid/namespaces/topics@2023-12-15-preview' existing = {
  name: destinationTopicName
  parent: eventGridNamespace
}

resource eventSendRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: destinationTopic
  name: guid(subscription().id, resourceGroup().id, topic.id, 'sendEventRole')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'd5a91429-5739-47e2-a06b-3470a27159e7')
    principalType: 'ServicePrincipal'
    principalId: topic.identity.principalId
  }
}

resource topic 'Microsoft.EventGrid/systemTopics@2023-12-15-preview' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    source: storage.id
    topicType: 'Microsoft.Storage.StorageAccounts'
  }
}

output name string = topic.name
