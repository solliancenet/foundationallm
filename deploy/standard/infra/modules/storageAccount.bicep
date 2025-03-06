/** Inputs **/
@description('Action Group Id for alerts')
param actionGroupId string

param adminGroupObjectId string

@description('Containers to create')
param containers array = []

@description('Flag to enable HNS')
param enableHns bool = false

@description('Flag specifying if this is a datalake storage account')
param isDataLake bool = false

@description('Location for all resources')
param location string

@description('Log Analytic Workspace Id to use for diagnostics')
param logAnalyticWorkspaceId string

param principalType string

@description('Private DNS Zones for private endpoint')
param privateDnsZones array

@description('Queues to create')
param queues array = []

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
    description: 'Alert on Storage Account Threshold - Account availability less than 99% for 5 minutes'
    evaluationFrequency: 'PT1M'
    metricName: 'Availability'
    name: 'storageUsage'
    operator: 'LessThan'
    severity: 1
    threshold: 99
    timeAggregation: 'Average'
    windowSize: 'PT5M'
  }
]

@description('The Resource logs to enable')
var logs = {
  blobServices: [ 'StorageRead', 'StorageWrite', 'StorageDelete' ]
  fileServices: [ 'StorageRead', 'StorageWrite', 'StorageDelete' ]
}

@description('Formatted untruncated resource name')
var formattedName = toLower(replace('${serviceType}-${resourceSuffix}', '-', ''))

@description('The Resource Name')
var name = substring(formattedName,0,min([length(formattedName),24]))

@description('The Resource Service Type token')
var serviceType = isDataLake ? 'adls' : 'sa'

/** Outputs **/
output name string = main.name

/** Resources **/
@description('The Storage Account')
resource main 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  kind: 'StorageV2'
  location: location
  name: name
  tags: tags

  identity: {
    type: 'SystemAssigned'
  }

  sku: {
    name: 'Standard_GZRS'
  }

  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    allowCrossTenantReplication: true
    allowSharedKeyAccess: false
    defaultToOAuthAuthentication: true
    isHnsEnabled: enableHns
    isNfsV3Enabled: false
    isSftpEnabled: false
    minimumTlsVersion: 'TLS1_2'
    publicNetworkAccess: 'Disabled'
    supportsHttpsTrafficOnly: true

    encryption: {
      keySource: 'Microsoft.Storage'
      requireInfrastructureEncryption: true

      services: {
        file: {
          enabled: true
          keyType: 'Account'
        }
        blob: {
          enabled: true
          keyType: 'Account'
        }
        queue: {
          enabled: true
        }
      }
    }

    networkAcls: {
      bypass: 'Logging, Metrics, AzureServices'
      defaultAction: 'Deny'
      ipRules: []
      virtualNetworkRules: []

      resourceAccessRules: [
        {
          tenantId: subscription().tenantId
          resourceId: subscriptionResourceId('Microsoft.Security/datascanners', 'storageDataScanner')
        }
      ]
    }

    sasPolicy: {
      sasExpirationPeriod: '00.04:00:00'
      expirationAction: 'Log'
    }
  }
}

@description('The blob service settings.')
resource blob 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: main
  name: 'default'

  properties: {
    changeFeed: { enabled: false }
    cors: { corsRules: [] }
    isVersioningEnabled: !isDataLake
    restorePolicy: { enabled: false }

    containerDeleteRetentionPolicy: {
      days: 30
      enabled: true
    }

    deleteRetentionPolicy: {
      allowPermanentDelete: false
      days: 30
      enabled: true
    }
  }
}

@description('Blob containers')
resource blobContainers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = [
  for container in containers: {
    name: container
    parent: blob
    properties: {
      publicAccess: 'None'
      metadata: null
    }
  }
]

@description('Diagnostic settings for the resource.')
resource blobServicesDiagnostics 'Microsoft.Insights/diagnosticSettings@2017-05-01-preview' = {
  scope: blob
  name: 'diag-blobServices'
  properties: {
    workspaceId: logAnalyticWorkspaceId
    logs: [for log in logs.blobServices: {
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

@description('The file service settings.')
resource file 'Microsoft.Storage/storageAccounts/fileServices@2023-01-01' = {
  name: 'default'
  parent: main

  properties: {
    cors: { corsRules: [] }
    protocolSettings: { smb: {} }

    shareDeleteRetentionPolicy: isDataLake ? null : {
      days: 30
      enabled: true
    }
  }
}

@description('Diagnostic settings for the resource.')
resource fileServicesDiagnostics 'Microsoft.Insights/diagnosticSettings@2017-05-01-preview' = {
  scope: file
  name: 'diag-fileServices'
  properties: {
    workspaceId: logAnalyticWorkspaceId
    logs: [for log in logs.fileServices: {
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

@description('The queue service settings.')
resource queue 'Microsoft.Storage/storageAccounts/queueServices@2023-01-01' = {
  name: 'default'
  parent: main

  properties: {
    cors: { corsRules: [] }
  }
}

@description('Queues')
resource storageQueues 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = [
  for q in queues: {
    name: q
    parent: queue
    properties: {
      metadata: null
    }
  }
]

@description('The table service settings.')
resource table 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  name: 'default'
  parent: main

  properties: {
    cors: { corsRules: [] }
  }
}

/** Nested Modules **/
@description('Metric alerts for the Resource')
module metricAlerts 'utility/metricAlerts.bicep' = {
  name: 'alert-${main.name}-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    alerts: alerts
    metricNamespace: 'Microsoft.Storage/storageaccounts'
    nameSuffix: name
    serviceId: main.id
    tags: tags
  }
}

@description('Private endpoint for the resource.')
module privateEndpoint 'utility/privateEndpoint.bicep' = [for zone in privateDnsZones: {
  name: 'pe-${main.name}-${zone.key}-${timestamp}'
  params: {
    groupId: zone.key
    location: location
    privateDnsZones: filter(privateDnsZones, item => item.key == zone.key)
    subnetId: subnetId
    tags: tags

    service: {
      id: main.id
      name: main.name
    }
  }
}]

@description('Storage Role assignments for admin group')
module storageRoleAssignments 'utility/roleAssignments.bicep' = {
  name: 'storageIAM-${timestamp}'
  scope: resourceGroup()
  params: {
    principalId: adminGroupObjectId
    principalType: 'Group'
    roleDefinitionIds: {
      'Storage Blob Data Contributor': 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
    }
  }
}

