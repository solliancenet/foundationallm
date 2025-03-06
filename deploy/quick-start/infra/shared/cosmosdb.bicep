param containers array
param databaseName string
param location string = resourceGroup().location
param name string
param tags object = {}

resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: name
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    databaseAccountOfferType: 'Standard'
    disableLocalAuth: true
    locations: [
      {
        failoverPriority: 0
        isZoneRedundant: false
        locationName: location
      }
    ]
  }
  tags: tags
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-04-15' = {
  parent: cosmosDb
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
  }
  tags: tags
}

resource cosmosContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = [
  for container in containers: if (container.defaultTtl == null) {
    parent: database
    name: container.name
    properties: {
      resource: {
        id: container.name
        partitionKey: {
          paths: [
            container.partitionKeyPath
          ]
          kind: 'Hash'
          version: 2
        }
      }
      options: {
        autoscaleSettings: {
          maxThroughput: container.maxThroughput
        }
      }
    }
    tags: tags
  }
]

resource cosmosContainerWithTtl 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = [
  for container in containers: if (container.defaultTtl != null) {
    parent: database
    name: container.name
    properties: {
      resource: {
        id: container.name
        partitionKey: {
          paths: [
            container.partitionKeyPath
          ]
          kind: 'Hash'
          version: 2
        }
        defaultTtl: container.defaultTtl
      }
      options: {
        autoscaleSettings: {
          maxThroughput: container.maxThroughput
        }
      }
    }
    tags: tags
  }
]

output endpoint string = cosmosDb.properties.documentEndpoint
output name string = cosmosDb.name
