metadata description = 'Creates an Azure Cosmos DB for NoSQL account.'
param connectionStringKey string = 'AZURE-COSMOS-CONNECTION-STRING'
param keyVaultName string
param location string = resourceGroup().location
param name string
param tags object = {}

module cosmos '../../cosmos/cosmos-account.bicep' = {
  name: 'cosmos-account'
  params: {
    name: name
    connectionStringKey: connectionStringKey
    location: location
    tags: tags
    keyVaultName: keyVaultName
    kind: 'GlobalDocumentDB'
  }
}

output connectionStringKey string = cosmos.outputs.connectionStringKey
output endpoint string = cosmos.outputs.endpoint
output id string = cosmos.outputs.id
output name string = cosmos.outputs.name
