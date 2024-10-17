/** Inputs **/
@description('Action Group Id for alerts')
param actionGroupId string

param deployments array = []

@description('Location for all resources')
param location string

@description('Log Analytic Workspace Id to use for diagnostics')
param logAnalyticWorkspaceId string

@description('Private DNS Zones for private endpoint')
param privateDnsZones array

@description('Resource suffix for all resources')
param resourceSuffix string

@description('Subnet Id for private endpoint')
param subnetId string

@description('Tags for all resources')
param tags object

@description('Timestamp for nested deployments')
param timestamp string = utcNow()

/** Locals **/
@description('Metric alerts for the resource.')
var alerts = [
  {
    description: 'Service availability less than 99% for 1 hour'
    evaluationFrequency: 'PT1M'
    metricName: 'SuccessRate'
    name: 'availability'
    operator: 'LessThan'
    severity: 0
    threshold: 99
    timeAggregation: 'Average'
    windowSize: 'PT1H'
  }
  {
    description: 'Service latency more than 1000ms for 1 hour'
    evaluationFrequency: 'PT1M'
    metricName: 'Latency'
    name: 'latency'
    operator: 'GreaterThan'
    severity: 0
    threshold: 1000
    timeAggregation: 'Average'
    windowSize: 'PT1H'
  }
]

@description('The Resource logs to enable')
var logs = [ 'Trace', 'RequestResponse', 'Audit' ]

@description('The Resource Name')
var name = '${serviceType}-${resourceSuffix}'

@description('The Resource Service Type token')
var serviceType = 'oai'

/** Outputs **/
output id string = main.id

@description('The Account Name')
output name string = main.name

@description('The Account Endpoint')
output endpoint string = main.properties.endpoint

/** Resources **/
resource main 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  kind: 'OpenAI'
  location: location
  name: name
  tags: tags

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    allowedFqdnList: []
    customSubDomainName: name
    disableLocalAuth: false
    dynamicThrottlingEnabled: false
    publicNetworkAccess: 'Disabled'
    restrictOutboundNetworkAccess: false
  }

  sku: {
    name: 'S0'
  }
}

@batchSize(1)
resource deployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = [for config in deployments: {
  name: config.name
  parent: main

  properties: {
    versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
    raiPolicyName: config.raiPolicyName
    model: config.model
  }

  sku: config.sku
}]

@description('Resource for configuring the diagnostics.')
resource diagnostics 'Microsoft.Insights/diagnosticSettings@2017-05-01-preview' = {
  scope: main
  name: 'diag-${serviceType}'
  properties: {
    workspaceId: logAnalyticWorkspaceId
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
@description('Resource for configuring the Key Vault metric alerts.')
module metricAlerts 'utility/metricAlerts.bicep' = {
  name: 'alert-${main.name}-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    alerts: alerts
    metricNamespace: 'Microsoft.CognitiveServices/accounts'
    nameSuffix: name
    serviceId: main.id
    tags: tags
  }
}

@description('Resource for configuring the private endpoint.')
module privateEndpoint 'utility/privateEndpoint.bicep' = {
  name: 'pe-${main.name}-${timestamp}'
  params: {
    groupId: 'account'
    location: location
    privateDnsZones: privateDnsZones
    subnetId: subnetId
    tags: tags

    service: {
      name: main.name
      id: main.id
    }
  }
  dependsOn: [deployment]
}
