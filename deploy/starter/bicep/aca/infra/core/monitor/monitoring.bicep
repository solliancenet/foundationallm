metadata description = 'Creates an Application Insights instance and a Log Analytics workspace.'
param applicationInsightsDashboardName string = ''
param applicationInsightsName string
param location string = resourceGroup().location
param logAnalyticsName string
param logAnalyticsSku string = 'PerGB2018'
param tags object = {}

module logAnalytics 'loganalytics.bicep' = {
  name: 'loganalytics'
  params: {
    name: logAnalyticsName
    location: location
    tags: tags
    sku: {
      name: logAnalyticsSku
    }
  }
}

module applicationInsights 'applicationinsights.bicep' = {
  name: 'applicationinsights'
  params: {
    name: applicationInsightsName
    location: location
    tags: tags
    dashboardName: applicationInsightsDashboardName
    logAnalyticsWorkspaceId: logAnalytics.outputs.id
  }
}

output applicationInsightsConnectionString string = applicationInsights.outputs.connectionString
output applicationInsightsInstrumentationKey string = applicationInsights.outputs.instrumentationKey
output applicationInsightsName string = applicationInsights.outputs.name
output logAnalyticsWorkspaceId string = logAnalytics.outputs.id
output logAnalyticsWorkspaceName string = logAnalytics.outputs.name
