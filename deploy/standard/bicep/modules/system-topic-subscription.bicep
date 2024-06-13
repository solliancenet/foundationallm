/*
This Bicep file defines a system topic subscription in Azure Event Grid. It
creates an event subscription for a system topic, which allows you to receive
events from various Azure services.

Parameters:
- appResourceGroup: The resource group where the application resources are located.
- destinationTopicName: The name of the destination topic where events will be sent.
- eventGridName: The name of the Event Grid namespace.
- filterPrefix: (Optional) A prefix to filter events based on their subject.
- includedEventTypes: An array of event types to include in the subscription.
- name: The name of the system topic subscription.
- timestamp: (Optional) The timestamp used for generating a unique name for the
  role assignment. Defaults to the current UTC time.
- topicName: The name of the system topic.

Resources:
- destinationTopic: An existing Event Grid topic resource.
- eventGridNamespace: An existing Event Grid namespace resource.
- topic: An existing system topic resource.
- resourceProviderSub: The system topic subscription resource.

Nested Modules:
- eventSendRole: A utility module for assigning the EventGrid Data Sender role
  to the system topic.

Note: This code assumes that the necessary Event Grid resources already exist
  and are provided as parameters.
*/
param appResourceGroup string
param destinationTopicName string
param eventGridName string
param filterPrefix string = ''
param includedEventTypes array
param advancedFilters array = []
param name string
param timestamp string = utcNow()
param topicName string

/** Data Sources **/
resource destinationTopic 'Microsoft.EventGrid/namespaces/topics@2023-12-15-preview' existing = {
  name: destinationTopicName
  parent: eventGridNamespace
}

resource eventGridNamespace 'Microsoft.EventGrid/namespaces@2023-12-15-preview' existing = {
  name: eventGridName
  scope: resourceGroup(appResourceGroup)
}

resource topic 'Microsoft.EventGrid/systemTopics@2023-12-15-preview' existing = {
  name: topicName
}

/** Resources **/
resource resourceProviderSub 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2023-12-15-preview' = {
  dependsOn: [ eventSendRole ]
  name: name
  parent: topic

  properties: {
    eventDeliverySchema: 'CloudEventSchemaV1_0'
    deliveryWithResourceIdentity: {
      destination: {
        endpointType: 'NamespaceTopic'
        properties: {
          resourceId: destinationTopic.id
        }
      }
      identity: {
        type: 'SystemAssigned'
      }
    }
    filter: {
      advancedFilters: advancedFilters
      enableAdvancedFilteringOnArrays: true
      includedEventTypes: includedEventTypes
      subjectBeginsWith: filterPrefix
    }
    retryPolicy: {
      eventTimeToLiveInMinutes: 1440
      maxDeliveryAttempts: 30
    }
  }
}

/** Nested Modules **/
module eventSendRole 'utility/roleAssignments.bicep' = {
  name: 'stSendRole-${name}-${timestamp}'
  scope: resourceGroup(appResourceGroup)
  params: {
    principalId: topic.identity.principalId
    roleDefinitionIds: {
      'EventGrid Data Sender': 'd5a91429-5739-47e2-a06b-3470a27159e7'
    }
  }
}
