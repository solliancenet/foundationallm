/** Inputs **/
// @description('AMPLS Name')
// param amplsName string

@description('The environment name token used in naming resources.')
param environmentName string

@description('Location used for all resources.')
param location string

@description('Log Analytics Workspace Id.')
param logAnalyticWorkspaceId string

@description('Project Name, used in naming resources.')
param project string

@description('Resource suffix')
param resourceSuffix string

@description('Tags for all resources')
param tags object

@description('Timestamp for nested deployments')
param timestamp string = utcNow()

/** Locals **/
@description('Formatted untruncated resource name')
var kvFormattedName = toLower('${kvServiceType}-${substring(resourceSuffix, 0, length(resourceSuffix) - 4)}')

@description('The Resource Name')
var kvTruncatedName = substring(kvFormattedName,0,min([length(kvFormattedName),20]))
var kvName = '${kvTruncatedName}-${substring(resourceSuffix, length(resourceSuffix) - 3, 3)}'

@description('The Resource Service Type token')
var kvServiceType = 'kv'

@description('Resource name.')
var name = 'ai-${resourceSuffix}'

/** Outputs **/
output name string = main.name
@description('Application Insights Connection String KeyVault Secret Uri.')
output aiConnectionStringSecretUri string = aiConnectionString.outputs.secretUri

/** Resources **/
@description('Resource representing an Azure Application Insights component.')
resource main 'microsoft.insights/components@2020-02-02' = {
  kind: 'web'
  location: location
  name: name

  properties: {
    Application_Type: 'web'
    DisableIpMasking: false
    DisableLocalAuth: false
    ForceCustomerStorageForProfiler: false
    IngestionMode: 'LogAnalytics'
    RetentionInDays: 30
    SamplingPercentage: 100
    WorkspaceResourceId: logAnalyticWorkspaceId
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }

  tags: {
    Environment: environmentName
    IaC: 'Bicep'
    Project: project
    Purpose: 'DevOps'
  }
}

/**
 * Creates a scoped service for private link integration with Azure Log Analytics.
 */
// resource scopedService 'microsoft.insights/privatelinkscopes/scopedresources@2021-07-01-preview' = {
//   name: '${amplsName}/amplss-${name}'
//   properties: {
//     linkedResourceId: main.id
//   }
// }

@description('Application Insights connection string.')
module aiConnectionString 'kvSecret.bicep' = {
  name: 'aiConn-${timestamp}'
  params: {
    kvName: kvName
    secretName: 'foundationallm-appinsights-connectionstring'
    secretValue: main.properties.ConnectionString
    tags: tags
  }
}
