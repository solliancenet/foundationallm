/** Inputs **/
@description('Action Group Id for alerts')
param actionGroupId string

@description('Location for all resources')
param location string

@description('Log Analytic Workspace Id to use for diagnostics')
param logAnalyticsWorkspaceId string

@description('Private DNS Zones for private endpoint')
param privateDnsZones array

@description('Resource suffix for all resources')
param resourceSuffix string

@description('Subnet Id for private endpoint')
param subnetId string
param sku string = 'standard'
@description('Tags for all resources')
param tags object

@description('Timestamp for nested deployments')
param timestamp string = utcNow()

/** Locals **/
@description('Metric alerts for the resource')
var alerts = [
  {
    description: 'Service latency greater than 5s for 1 hour'
    evaluationFrequency: 'PT1M'
    metricName: 'SearchLatency'
    name: 'latency'
    operator: 'GreaterThan'
    severity: 0
    threshold: 5
    timeAggregation: 'Average'
    windowSize: 'PT1H'
  }
  {
    description: 'Service throttled search queries greater than 25% for 1 hour'
    evaluationFrequency: 'PT1M'
    metricName: 'ThrottledSearchQueriesPercentage'
    name: 'throttling'
    operator: 'GreaterThan'
    severity: 0
    threshold: 25
    timeAggregation: 'Average'
    windowSize: 'PT1H'
  }
]

@description('The Resource logs to enable')
var logs = [
  'OperationLogs'
]

@description('Formatted untruncated resource name')
var formattedName = toLower('${serviceType}-${resourceSuffix}')

@description('The Resource Name')
var name = substring(formattedName, 0, min([ length(formattedName), 60 ]))

@description('The Resource Service Type token')
var serviceType = 'search'

/** Outputs **/

/** Resources **/
@description('The Azure AI Search resource.')
resource main 'Microsoft.Search/searchServices@2023-11-01' = {
  location: location
  name: name
  tags: tags

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    disableLocalAuth: true
    hostingMode: 'default'
    partitionCount: 1
    publicNetworkAccess: 'disabled'
    replicaCount: 1
    semanticSearch: 'disabled'

    encryptionWithCmk: {
      enforcement: 'Disabled'
    }

    networkRuleSet: {
      ipRules: []
    }
  }

  sku: {
    name: sku
  }
}

@description('Diagnostic settings for the resource')
resource diagnostics 'Microsoft.Insights/diagnosticSettings@2017-05-01-preview' = {
  scope: main
  name: 'diag-${serviceType}'
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [for log in logs: {
      category: log
      enabled: true
    }]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

/** Nested Modules **/
@description('Metric alerts for the resource')
module metricAlerts 'utility/metricAlerts.bicep' = {
  name: 'alerts-${serviceType}-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    alerts: alerts
    metricNamespace: 'Microsoft.Search/searchServices'
    nameSuffix: name
    serviceId: main.id
    tags: tags
  }
}

@description('Private endpoint for the resource')
module privateEndpoint 'utility/privateEndpoint.bicep' = {
  name: 'pe-${serviceType}-${timestamp}'
  params: {
    groupId: 'searchService'
    location: location
    privateDnsZones: privateDnsZones
    subnetId: subnetId
    tags: tags

    service: {
      id: main.id
      name: main.name
    }
  }
}
