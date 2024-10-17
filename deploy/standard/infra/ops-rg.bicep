/** Inputs **/
@description('Administrator Object Id')
param administratorObjectId string

@description('Administrator principal type.')
param administratorPrincipalType string = 'Group'

@description('The environment name token used in naming resources.')
param environmentName string

param hubResourceGroup string
param hubSubscriptionId string = subscription().subscriptionId

@description('Location used for all resources.')
param location string

@description('Project Name, used in naming resources.')
param project string

@description('Timestamp used in naming nested deployments.')
param timestamp string = utcNow()

@description('Virtual Network ID, used to find the subnet IDs.')
param vnetId string

/** Locals **/
@description('Formatted untruncated resource name')
var kvFormattedName = toLower('${kvServiceType}-${substring(kvResourceSuffix, 0, length(kvResourceSuffix) - 4)}')

@description('The Resource Name')
var kvTruncatedName = substring(kvFormattedName,0,min([length(kvFormattedName),20]))
var kvName = '${kvTruncatedName}-${substring(kvResourceSuffix, length(kvResourceSuffix) - 3, 3)}'

@description('KeyVault resource suffix')
var kvResourceSuffix = resourceSuffix

@description('The Resource Service Type token')
var kvServiceType = 'kv'

@description('Resource Suffix used in naming resources.')
var resourceSuffix = '${project}-${environmentName}-${location}-${workload}'

@description('Tags for all resources')
var tags = {
  'azd-env-name': environmentName
  'iac-type': 'bicep'
  'project-name': project
  Purpose: 'DevOps'
}

@description('Workload Token used in naming resources.')
var workload = 'ops'

@description('Private DNS Zones for Azure Monitor Private Link Scope')
var zonesAmpls = filter(
  dnsZones.outputs.ids,
  (zone) => contains([ 'monitor', 'blob', 'ods', 'oms', 'agentsvc' ], zone.key)
)

/** Outputs **/
output actionGroupId string = actionGroup.outputs.id
output keyVaultName string = keyVault.outputs.name
output logAnalyticsWorkspaceId string = logAnalytics.outputs.id
output monitorWorkspaceName string = logAnalytics.outputs.monitorWorkspaceName

/** Resources **/
@description('User Assigned Identity for App Configuration')
resource uaiAppConfig 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  location: location
  name: 'uai-appconfig-${resourceSuffix}'
  tags: tags
}

/** Modules **/
@description('Read DNS Zones')
module dnsZones 'modules/utility/dnsZoneData.bicep' = {
  name: 'dnsZones-${timestamp}'
  scope: resourceGroup(hubSubscriptionId, hubResourceGroup)
  params: {
    location: location
  }
}

@description('Azure Monitor Action Group')
module actionGroup 'modules/actionGroup.bicep' = {
  name: 'actionGroup-${timestamp}'
  params: {
    environmentName: environmentName
    project: project
    resourceSuffix: resourceSuffix
  }
}

@description('Azure Monitor Private Link Scope')
module ampls 'modules/ampls.bicep' = {
  name: 'ampls-${timestamp}'
  params: {
    environmentName: environmentName
    location: location
    privateDnsZones: zonesAmpls
    project: project
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/ops'
  }
}

@description('App Configuration')
module appConfig 'modules/appConfig.bicep' = {
  dependsOn: [ uaiAppConfigRoleAssignments ]
  name: 'appConfig-${timestamp}'
  params: {
    administratorObjectId: administratorObjectId
    administratorPrincipalType: administratorPrincipalType
    actionGroupId: actionGroup.outputs.id
    location: location
    logAnalyticWorkspaceId: logAnalytics.outputs.id
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => zone.key == 'configuration_stores')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/ops'
    tags: tags
    uaiId: uaiAppConfig.id
    vaultName: keyVault.outputs.name
  }
}

@description('Application Insights')
module applicationInsights 'modules/applicationInsights.bicep' = {
  name: 'appInsights-${timestamp}'
  params: {
    amplsName: ampls.outputs.name
    environmentName: environmentName
    location: location
    logAnalyticWorkspaceId: logAnalytics.outputs.id
    project: project
    resourceSuffix: resourceSuffix
    tags: tags
  }
  dependsOn: [ keyVault ]
}

module dashboard 'modules/dashboard-web.bicep' = {
  name: 'dashboard-${timestamp}'
  params: {
    name: 'dash-${resourceSuffix}'
    applicationInsightsName: applicationInsights.outputs.name
    location: location
    tags: tags
  }
}

@description('Key Vault')
module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVault-${timestamp}'
  params: {
    actionGroupId: actionGroup.outputs.id
    administratorObjectId: administratorObjectId
    administratorPrincipalType: administratorPrincipalType
    allowAzureServices: true
    location: location
    logAnalyticWorkspaceId: logAnalytics.outputs.id
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => zone.key == 'vault')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/ops'
    tags: tags
  }
}

@description('Log Analytics')
module logAnalytics 'modules/logAnalytics.bicep' = {
  name: 'logAnalytics-${timestamp}'
  params: {
    actionGroupId: actionGroup.outputs.id
    environmentName: environmentName
    location: location
    project: project
    resourceSuffix: resourceSuffix

    ampls: {
      id: ampls.outputs.id
      name: ampls.outputs.name
    }
  }
}

// See: https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
module uaiAppConfigRoleAssignments 'modules/utility/roleAssignments.bicep' = {
  name: 'uaiAppConfigRoleAssignments-${timestamp}'
  params: {
    principalId: uaiAppConfig.properties.principalId
    roleDefinitionIds: {
      'Key Vault Crypto Service Encryption User': 'e147488a-f6f5-4113-8e2d-b22465e65bf6'
    }
  }
}

@description('Placeholder configuration setting for CSV file')
module csvFileSecret 'modules/kvSecret.bicep' = {
  name: 'csvFileSecret-${timestamp}'
  params: {
    kvName: kvName
    secretName: 'foundationallm-langchain-csvfile-url'
    secretValue: 'FIXME'
    tags: tags
  }
  dependsOn: [ keyVault ]
}

@description('Placeholder configuration setting for Test DB Password')
module dbPasswdSecret 'modules/kvSecret.bicep' = {
  name: 'dbPasswdSecret-${timestamp}'
  params: {
    kvName: kvName
    secretName: 'foundationallm-langchain-sqldatabase-testdb-password'
    secretValue: 'FIXME'
    tags: tags
  }
  dependsOn: [ keyVault ]
}
