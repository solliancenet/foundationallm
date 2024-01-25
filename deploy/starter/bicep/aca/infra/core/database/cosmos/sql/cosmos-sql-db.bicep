metadata description = 'Creates an Azure Cosmos DB for NoSQL account with a database.'
param accountName string
param connectionStringKey string = 'AZURE-COSMOS-CONNECTION-STRING'
param containers array = []
param databaseName string
param keyVaultName string
param location string = resourceGroup().location
param principalIds array = []
param tags object = {}

module cosmos 'cosmos-sql-account.bicep' = {
  name: 'cosmos-sql-account'
  params: {
    connectionStringKey: connectionStringKey
    keyVaultName: keyVaultName
    location: location
    name: accountName
    tags: tags
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-05-15' = {
  name: '${accountName}/${databaseName}'
  properties: {
    resource: { id: databaseName }
  }

  resource list 'containers' = [for container in containers: {
    name: container.name
    properties: {
      options: {
        autoscaleSettings: { maxThroughput: container.maxThroughput }
      }

      resource: {
        id: container.id
        partitionKey: {
          kind: 'Hash'
          paths: [ container.partitionKey ]
          version: 2
        }
      }
    }
  }]

  dependsOn: [
    cosmos
  ]
}

module roleDefinition 'cosmos-sql-role-def.bicep' = {
  name: 'cosmos-sql-role-definition'
  params: {
    accountName: accountName
  }
  dependsOn: [
    cosmos
    database
  ]
}

// We need batchSize(1) here because sql role assignments have to be done sequentially
@batchSize(1)
module userRole 'cosmos-sql-role-assign.bicep' = [for principalId in principalIds: if (!empty(principalId)) {
  name: 'cosmos-sql-user-role-${uniqueString(principalId)}'
  params: {
    accountName: accountName
    roleDefinitionId: roleDefinition.outputs.id
    principalId: principalId
  }
  dependsOn: [
    cosmos
    database
  ]
}]

output accountId string = cosmos.outputs.id
output accountName string = cosmos.outputs.name
output connectionStringKey string = cosmos.outputs.connectionStringKey
output databaseName string = databaseName
output endpoint string = cosmos.outputs.endpoint
output roleDefinitionId string = roleDefinition.outputs.id
