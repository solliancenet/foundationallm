metadata description = 'Creates an Azure App Configuration instance.'
param name string
param location string = resourceGroup().location
param tags object = {}
param sku object = {
  name: 'standard'
}

resource store 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  location: location
  name: name
  sku: sku
  tags: tags

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    encryption: {}
    disableLocalAuth: false
    softDeleteRetentionInDays: 7
    enablePurgeProtection: false
  }
}
