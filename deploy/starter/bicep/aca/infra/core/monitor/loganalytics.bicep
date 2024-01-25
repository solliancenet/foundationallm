metadata description = 'Creates a Log Analytics workspace.'
param name string
param location string = resourceGroup().location
param tags object = {}
param sku object = {
  name: 'PerGB2018'
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: name
  location: location
  tags: tags
  properties: any({
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    retentionInDays: 30
    sku: sku

    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
      searchVersion: 1
    }
    workspaceCapping: {
      dailyQuotaGb: -1
    }
  })
}

output id string = logAnalytics.id
output name string = logAnalytics.name
