param actionGroupId string
// param ampls object
param environmentName string
param location string
param project string

@minLength(1)
param resourceSuffix string

var name = 'la-${resourceSuffix}'

output id string = main.id
output monitorWorkspaceName string = monitor.name

var alerts = [
  {
    description: 'Data ingestion is exceeding the ingestion rate limit.'
    evaluationFrequency: 'PT5M'
    name: 'RateLimit'
    query: '_LogOperation | where Category == "Ingestion" | where Operation has "Ingestion rate"'
    severity: 2
    windowSize: 'PT5M'
  }
  {
    description: 'Data ingestion has hit the daily cap.'
    evaluationFrequency: 'PT5M'
    name: 'IngestionCap'
    query: '_LogOperation | where Category == "Ingestion" | where Operation has "Data collection"'
    severity: 2
    windowSize: 'PT5M'
  }
  {
    description: 'Operational issues.'
    evaluationFrequency: 'P1D'
    name: 'Issues'
    query: '_LogOperation | where Level == "Warning"'
    severity: 3
    windowSize: 'P1D'
  }
]

var solutions = [
  {
    name: 'ContainerInsights'
    product: 'OMSGallery/ContainerInsights'
    publisher: 'Microsoft'
  }
  {
    name: 'Security'
    product: 'OMSGallery/Security'
    publisher: 'Microsoft'
  }
  {
    name: 'SecurityCenterFree'
    product: 'OMSGallery/SecurityCenterFree'
    publisher: 'Microsoft'
  }
  {
    name: 'SQLAdvancedThreatProtection'
    product: 'OMSGallery/SQLAdvancedThreatProtection'
    publisher: 'Microsoft'
  }
  {
    name: 'SQLVulnerabilityAssessment'
    product: 'OMSGallery/SQLVulnerabilityAssessment'
    publisher: 'Microsoft'
  }
]

var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'DevOps'
}

/**
 * Resource representing a Log Analytics workspace.
 */
resource main 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: name
  location: location
  tags: tags

  properties: {
    forceCmkForQuery: false
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    retentionInDays: 30

    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
      disableLocalAuth: false
    }

    sku: {
      name: 'PerGB2018'
    }

    workspaceCapping: {
      dailyQuotaGb: -1
    }
  }
}

resource monitor 'Microsoft.Monitor/accounts@2023-04-03' = {
  name: 'mon-${resourceSuffix}'
  location: location
}

/**
 * Resource for configuring diagnostic settings for Log Analytics workspace.
 */
resource diagnostics 'Microsoft.Insights/diagnosticSettings@2017-05-01-preview' = {
  scope: main
  name: 'diag-la'
  properties: {
    workspaceId: main.id
    logs: [
      {
        category: 'Audit'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

/**
 * Creates a scoped service for private link integration with Azure Log Analytics.
 */
// resource scopedService 'microsoft.insights/privatelinkscopes/scopedresources@2021-07-01-preview' = {
//   name: '${ampls.name}/amplss-${name}'
//   properties: {
//     linkedResourceId: main.id
//   }
// }

/*
  This resource block deploys a Log Analytics solution.
  It creates a Microsoft.OperationsManagement/solutions resource and associates it
  with the specified workspace.
  The solution is defined based on the input solutions array.
*/
resource solution 'Microsoft.OperationsManagement/solutions@2015-11-01-preview' = [for solution in solutions: {
  location: location
  name: '${solution.name}(${main.name})'

  plan: {
    name: '${solution.name}(${main.name})'
    product: solution.product
    promotionCode: ''
    publisher: solution.publisher
  }

  properties: {
    workspaceResourceId: main.id
  }
}]

/**
 * Resource definition for creating Log Analytics alerts.
 */
resource alert 'microsoft.insights/scheduledqueryrules@2023-03-15-preview' = [for alert in alerts: {
  kind: 'LogAlert'
  location: location
  name: 'alert-${alert.name}-${name}'
  tags: tags

  properties: {
    autoMitigate: false
    checkWorkspaceAlertsStorageConfigured: false
    displayName: alert.description
    enabled: true
    evaluationFrequency: alert.evaluationFrequency
    scopes: [ main.id ]
    severity: alert.severity
    skipQueryValidation: false
    windowSize: alert.windowSize

    actions: {
      actionGroups: [ actionGroupId ]
    }

    criteria: {
      allOf: [
        {
          operator: 'GreaterThan'
          query: alert.query
          resourceIdColumn: '_ResourceId'
          threshold: 0
          timeAggregation: 'Count'

          failingPeriods: {
            minFailingPeriodsToAlert: 1
            numberOfEvaluationPeriods: 1
          }
        }
      ]
    }
  }
}]
