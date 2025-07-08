/** Inputs **/
@description('Action Group to use for alerts.')
param actionGroupId string

@description('Administrator Object Id')
param administratorObjectId string

@description('APP Resource Group Name')
param appResourceGroupName string
param opsResourceGroupName string
param storageResourceGroupName string

@description('The environment name token used in naming resources.')
param environmentName string

param dnsResourceGroup string
param dnsSubscriptionId string = subscription().subscriptionId

param instanceId string

@description('AKS namespace')
param k8sNamespace string

@description('Location used for all resources.')
param location string

@description('Log Analytics Workspace Id to use for diagnostics')
param logAnalyticsWorkspaceId string

param principalType string

@description('Project Name, used in naming resources.')
param project string

@description('Timestamp used in naming nested deployments.')
param timestamp string = utcNow()

@description('Virtual Network ID, used to find the subnet IDs.')
param vnetId string

/** Locals **/
@description('Resource Suffix used in naming resources.')
var resourceSuffix = '${project}-${environmentName}-${location}-${workload}'
var resourceToken = toLower(uniqueString(resourceGroup().id, project, environmentName, location, workload))

@description('Resource Suffix used in naming resources.')
var opsResourceSuffix = '${project}-${environmentName}-${location}-ops'

param services array
var serviceNames = [for service in services: service.name]

var backendServices = {
  'context-api': { displayName: 'ContextAPI' }
}
var backendServiceNames = [for service in items(backendServices): service.key]

@description('Tags for all resources')
var tags = {
  'azd-env-name': environmentName
  'iac-type': 'bicep'
  'project-name': project
  Purpose: 'Services'
}

@description('Workload Token used in naming resources.')
var workload = 'ctx'

/** Nested Modules **/
@description('Read DNS Zones')
module dnsZones 'modules/utility/dnsZoneData.bicep' = {
  name: 'dnsZones-${timestamp}'
  scope: resourceGroup(dnsSubscriptionId, dnsResourceGroup)
  params: {
    location: location
  }
}

module appInsightsData 'modules/utility/appInsightsData.bicep' = {
  name: 'appInsights-${timestamp}'
  scope: resourceGroup(opsResourceGroupName)
  params: {
    resourceSuffix: opsResourceSuffix
  }
}

resource aksBackend 'Microsoft.ContainerService/managedClusters@2024-01-02-preview' existing = {
  name: 'aks-${resourceSuffix}-backend'
  scope: resourceGroup(appResourceGroupName)
}

module contextStore 'modules/storageAccount.bicep' = {
  name: 'context-store-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    adminGroupObjectId: administratorObjectId
    enableHns: true
    isDataLake: true
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    principalType: principalType
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => contains(['blob', 'dfs'], zone.key))
    resourceSuffix: resourceToken
    subnetId: '${vnetId}/subnets/storage'
    tags: tags
    containers: [
      instanceId
    ]
  }
}

@batchSize(3)
module serviceResources 'modules/service.bicep' = [
  for service in items(backendServices): {
    name: 'beSvc-${service.key}-${timestamp}'
    scope: resourceGroup(appResourceGroupName)
    params: {
      location: location
      namespace: k8sNamespace
      oidcIssuerUrl: aksBackend.properties.oidcIssuerProfile.issuerURL
      opsResourceGroupName: opsResourceGroupName
      opsResourceSuffix: opsResourceSuffix
      resourceSuffix: resourceSuffix
      secretName: services[indexOf(serviceNames, service.key)].apiKeySecretName
      serviceName: service.key
      storageResourceGroupName: resourceGroup().name
      tags: tags
    }
  }
]

/** Data Sources **/
resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts@2024-02-15-preview' existing = {
  name: 'cdb-${project}-${environmentName}-${location}-storage'
  scope: resourceGroup(storageResourceGroupName)
}

module cosmosRoles './modules/sqlRoleAssignments.bicep' = {
  scope: resourceGroup(storageResourceGroupName)
  name: 'core-api-cosmos-role'
  params: {
    accountName: cosmosDb.name
    principalId: serviceResources[indexOf(backendServiceNames, 'context-api')].outputs.servicePrincipalId
    roleDefinitionIds: {
      'Cosmos DB Built-in Data Contributor': '00000000-0000-0000-0000-000000000002'
    }
  }
  dependsOn: [serviceResources]
}

module contextRgRoles 'modules/utility/roleAssignments.bicep' = {
  name: 'IAM-context-${timestamp}'
  scope: resourceGroup()
  params: {
    principalId: serviceResources[indexOf(backendServiceNames, 'context-api')].outputs.servicePrincipalId
    roleDefinitionIds: {
      'Storage Blob Data Contributor': 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
      'Azure ContainerApps Session Executor': '0fb8eba5-a2bb-4abe-b1c1-49dfad359bb0'
    }
  }
}

module opsRgRoles 'modules/utility/roleAssignments.bicep' = {
  name: 'opsIAM-context-${timestamp}'
  scope: resourceGroup(opsResourceGroupName)
  params: {
    principalId: serviceResources[indexOf(backendServiceNames, 'context-api')].outputs.servicePrincipalId
    roleDefinitionIds: {
      'App Configuration Data Reader': '516239f1-63e1-4d78-a4de-a74fb236a071'
      'Key Vault Secrets User': '4633458b-17de-408a-b874-0445c86b69e6'
    }
  }
}

resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-10-02-preview' = {
  name: 'cae-${resourceSuffix}'
  location: location
  sku: {
    name: 'Consumption'
  }
  properties: {
    appInsightsConfiguration: {
      connectionString: appInsightsData.outputs.appInsightsConnectionString
    }

    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspaceId
      }
    }

    publicNetworkAccess: 'Disabled'
    vnetConfiguration: {
      infrastructureSubnetId: '${vnetId}/subnets/services'
      internal: true
    }
  }
}

resource containerAppEnvPE 'Microsoft.App/managedEnvironments/privateEndpointConnections@2024-10-02-preview' = {
  parent: containerAppEnv
  name: 'pe-cae-${resourceSuffix}'
  properties: {
    privateEndpoint: {}
    privateLinkServiceConnectionState: {
      actionsRequired: 'SessionPoolPLE'
      description: 'SessionPoolPLE'
      status: 'Approved'
    }
  }
}

resource sessionPool 'Microsoft.App/sessionPools@2024-10-02-preview' = {
  name: 'casp-${resourceSuffix}'
  location: location
  properties: {
    containerType: 'CustomContainer'
    environmentId: containerAppEnv.id
    customContainerTemplate: {
      containers: [
        {
          args: []
          command: []
          env: []
          image: 'cropseastus2svinternal.azurecr.io/python-codesession-api:0.9.7'
          name: 'python-codesession-api'
          resources: {
            cpu: 1
            memory: '0.25Gi'
          }
        }
      ]
      ingress: {
        targetPort: 80
      }
    }
  }
}


output contextStoreName string = contextStore.outputs.name
output sessionPoolEndpoint string = sessionPool.properties.poolManagementEndpoint
