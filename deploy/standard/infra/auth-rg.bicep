/** Inputs **/
@description('Action Group to use for alerts.')
param actionGroupId string

@description('Administrator Object Id')
param administratorObjectId string

@description('APP Resource Group Name')
param appResourceGroupName string
param opsResourceGroupName string

param authAppRegistrationInstance string
param authAppRegistrationTenantId string
param authAppRegistrationClientId string
param instanceId string

@description('The environment name token used in naming resources.')
param environmentName string

param hubResourceGroup string
param hubSubscriptionId string = subscription().subscriptionId

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

var services = {
  'authorization-api': { displayName: 'AuthorizationAPI' }
}

var authSecrets = [
  {
    name: 'foundationallm-authorizationapi-entra-instance'
    value: authAppRegistrationInstance
  }
  {
    name: 'foundationallm-authorizationapi-entra-tenantid'
    value: authAppRegistrationTenantId
  }
  {
    name: 'foundationallm-authorizationapi-entra-clientid'
    value: authAppRegistrationClientId
  }
  {
    name: 'foundationallm-authorizationapi-instanceids'
    value: instanceId
  }
]

@description('Tags for all resources')
var tags = {
  'azd-env-name': environmentName
  'iac-type': 'bicep'
  'project-name': project
  Purpose: 'Services'
}

@description('Workload Token used in naming resources.')
var workload = 'svc'

/** Nested Modules **/
@description('Read DNS Zones')
module dnsZones 'modules/utility/dnsZoneData.bicep' = {
  name: 'dnsZones-${timestamp}'
  scope: resourceGroup(hubSubscriptionId, hubResourceGroup)
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

module authStore 'modules/storageAccount.bicep' = {
  name: 'auth-store-${timestamp}'
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
    subnetId: '${vnetId}/subnets/auth'
    tags: tags
    containers: [
      'role-assignments'
      'policy-assignments'
      'secret-keys'
    ]
  }
}

module authKeyvault 'modules/keyVault.bicep' = {
  name: 'auth-kv-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    administratorObjectId: administratorObjectId
    administratorPrincipalType: 'Group'
    allowAzureServices: false
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => zone.key == 'vault')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/auth'
    tags: tags
  }
}

module authKvSecret 'modules/kvSecret.bicep' = [
  for (secret, i) in authSecrets: {
    name: 'authSecret${i}-${timestamp}'
    params: {
      kvName: authKeyvault.outputs.name
      secretName: secret.name
      secretValue: secret.value
      tags: tags
    }
  }
]

module appInsightsSecret 'modules/kvSecret.bicep' = {
  name: 'appInsightsSecret-${timestamp}'
  params: {
    kvName: authKeyvault.outputs.name
    secretName: 'foundationallm-authorizationapi-appinsights-connectionstring'
    secretValue: appInsightsData.outputs.appInsightsConnectionString
    tags: tags
  }
}

module authStoreSecret 'modules/kvSecret.bicep' = {
  name: 'authStoreSecret-${timestamp}'
  params: {
    kvName: authKeyvault.outputs.name
    secretName: 'foundationallm-authorizationapi-storage-accountname'
    secretValue: authStore.outputs.name
    tags: tags
  }
}

@batchSize(3)
module serviceResources 'modules/authService.bicep' = [
  for service in items(services): {
    name: 'beSvc-${service.key}-${timestamp}'
    params: {
      resourceSuffix: resourceSuffix
      serviceName: service.key
      location: location
      namespace: k8sNamespace
      oidcIssuerUrl: aksBackend.properties.oidcIssuerProfile.issuerURL
      tags: tags
    }
  }
]
