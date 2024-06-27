param name string
param destinationTopicName string
param eventGridName string
param location string = resourceGroup().location
param tags object = {}
param appConfigAccountName string

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-08-01-preview' existing = {
  name: appConfigAccountName
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
    source: appConfig.id
    topicType: 'Microsoft.AppConfiguration.ConfigurationStores'
  }
}

output name string = topic.name
