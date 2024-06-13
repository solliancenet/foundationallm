/** Inputs **/
@description('Action Group to use for alerts.')
param actionGroupId string

@description('Administrator Object Id')
param administratorObjectId string

@description('Chat UI OIDC Client Secret')
@secure()
param chatUiClientSecret string

@description('Core API OIDC Client Secret')
@secure()
param coreApiClientSecret string

@description('DNS Resource Group Name')
param dnsResourceGroupName string

@description('The environment name token used in naming resources.')
param environmentName string

@description('AKS namespace')
param k8sNamespace string

@description('Location used for all resources.')
param location string

@description('Log Analytics Workspace Id to use for diagnostics')
param logAnalyticsWorkspaceId string

@description('Log Analytics Workspace Resource Id to use for diagnostics')
param logAnalyticsWorkspaceResourceId string

@description('Management UI OIDC Client Secret')
@secure()
param managementUiClientSecret string

@description('Management API OIDC Client Secret')
@secure()
param managementApiClientSecret string

@description('Networking Resource Group Name')
param networkingResourceGroupName string

@description('OPS Resource Group name')
param opsResourceGroupName string

@description('Project Name, used in naming resources.')
param project string

@description('Storage Resource Group name')
param storageResourceGroupName string

@description('Timestamp used in naming nested deployments.')
param timestamp string = utcNow()

@description('Vectorization API OIDC Client Secret')
@secure()
param vectorizationApiClientSecret string

@description('Vectorization Resource Group name')
param vectorizationResourceGroupName string
param vnetName string

@description('OpenAI Resource Group name')
param openAiResourceGroupName string

/** Locals **/
@description('KeyVault resource suffix')
var opsResourceSuffix = '${project}-${environmentName}-${location}-ops'
var storageResourceSuffix = '${project}-${environmentName}-${location}-storage'

@description('Resource Suffix used in naming resources.')
var resourceSuffix = '${project}-${environmentName}-${location}-${workload}'

@description('Tags for all resources')
var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'Services'
}

var backendServices = {
  'gateway-api': { displayName: 'GatewayAPI' }
  'agent-hub-api': { displayName: 'AgentHubAPI' }
  'core-job': { displayName: 'CoreWorker' }
  'data-source-hub-api': { displayName: 'DataSourceHubAPI' }
  'gatekeeper-api': { displayName: 'GatekeeperAPI' }
  'gatekeeper-integration-api': { displayName: 'GatekeeperIntegrationAPI' }
  'langchain-api': { displayName: 'LangChainAPI' }
  'prompt-hub-api': { displayName: 'PromptHubAPI' }
  'orchestration-api': { displayName: 'OrchestrationAPI' }
  'semantic-kernel-api': { displayName: 'SemanticKernelAPI' }
  'vectorization-job': { displayName: 'VectorizationWorker' }
}
var backendServiceNames = [for service in items(backendServices): service.key]

var chatUiService = { 'chat-ui': { displayName: 'Chat' } }
var coreApiService = { 'core-api': { displayName: 'CoreAPI' } }
var vectorizationApiService = { 'vectorization-api': { displayName: 'VectorizationAPI' } }
var vecServiceNames = [for service in items(vectorizationApiService): service.key]

var managementUiService = { 'management-ui': { displayName: 'ManagementUI' } }
var managementApiService = { 'management-api': { displayName: 'ManagementAPI' } }

@description('Workload Token used in naming resources.')
var workload = 'svc'

/** Outputs **/

/** Data Sources **/
resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts@2024-02-15-preview' existing = {
  name: 'cdb-${project}-${environmentName}-${location}-storage'
  scope: resourceGroup(storageResourceGroupName)
}

module network 'modules/utility/virtualNetworkData.bicep' = {
  name: 'network-${resourceSuffix}-${timestamp}'
  scope: resourceGroup(networkingResourceGroupName)
  params: {
    vnetName: vnetName
    subnetNames: [
      'FLLMBackend'
      'FLLMFrontend'
      'FLLMServices'
    ]
  }
}

var subnets = reduce(
  map(network.outputs.subnets, subnet => {
      '${subnet.name}': {
        id: subnet.id
        addressPrefix: subnet.addressPrefix
      }
    }),
  {},
  (cur, acc) => union(cur, acc)
)

/** Resources **/

/** Nested Modules **/
module aksBackend 'modules/aks.bicep' = {
  name: 'aksBackend-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    admnistratorObjectIds: [ administratorObjectId ]
    dnsResourceGroupName: dnsResourceGroupName
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    logAnalyticWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    networkingResourceGroupName: networkingResourceGroupName
    opsResourceGroupName: opsResourceGroupName
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => contains([ 'aks' ], zone.key))
    resourceSuffix: '${resourceSuffix}-backend'
    subnetId: subnets.FLLMBackend.id
    subnetIdPrivateEndpoint: subnets.FLLMServices.id
    tags: tags
  }
}

module aksFrontend 'modules/aks.bicep' = {
  name: 'aksFrontend-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    admnistratorObjectIds: [ administratorObjectId ]
    dnsResourceGroupName: dnsResourceGroupName
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    logAnalyticWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    networkingResourceGroupName: networkingResourceGroupName
    opsResourceGroupName: opsResourceGroupName
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => contains([ 'aks' ], zone.key))
    resourceSuffix: '${resourceSuffix}-frontend'
    subnetId: subnets.FLLMFrontend.id
    subnetIdPrivateEndpoint: subnets.FLLMServices.id
    tags: tags
  }
}

module dnsZones 'modules/utility/dnsZoneData.bicep' = {
  name: 'dnsZones-${timestamp}'
  scope: resourceGroup(dnsResourceGroupName)
  params: {
    location: location
  }
}

module eventgrid 'modules/eventgrid.bicep' = {
  name: 'eventgrid-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    kvResourceSuffix: opsResourceSuffix
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    opsResourceGroupName: opsResourceGroupName
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => contains([ 'eventgrid' ], zone.key))
    resourceSuffix: resourceSuffix
    subnetId: subnets.FLLMServices.id
    topics: [ 'storage', 'vectorization', 'configuration' ]
    tags: tags
  }
}

module configTopic 'modules/config-system-topic.bicep' = {
  name: 'configTopic-${timestamp}'
  scope: resourceGroup(opsResourceGroupName)
  params: {
    actionGroupId: actionGroupId
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    resourceSuffix: opsResourceSuffix
    tags: tags
  }
}

module storageTopic 'modules/storage-system-topic.bicep' = {
  name: 'storageTopic-${timestamp}'
  scope: resourceGroup(storageResourceGroupName)
  params: {
    actionGroupId: actionGroupId
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    resourceSuffix: storageResourceSuffix
    tags: tags
  }
}

module storageSub 'modules/system-topic-subscription.bicep' = {
  name: 'storageSub-${timestamp}'
  params: {
    name: 'foundationallm-storage'
    appResourceGroup: resourceGroup().name
    destinationTopicName: 'storage'
    eventGridName: eventgrid.outputs.name
    filterPrefix: '/blobServices/default/containers/resource-provider/blobs'
    includedEventTypes: [
      'Microsoft.Storage.BlobCreated'
      'Microsoft.Storage.BlobDeleted'
    ]
    advancedFilters: [
      {
        key: 'subject'
        operatorType: 'StringNotEndsWith'
        values: [
          '_agent-references.json'
          '_data-source-references.json'
          '_prompt-references.json'
        ]
      }
    ]
    topicName: storageTopic.outputs.name
  }
  scope: resourceGroup(storageResourceGroupName)
  dependsOn: [eventgrid, storageTopic]
}

module configSub 'modules/system-topic-subscription.bicep' = {
  name: 'configSub-${timestamp}'
  params: {
    name: 'app-config'
    appResourceGroup: resourceGroup().name
    destinationTopicName: 'configuration'
    eventGridName: eventgrid.outputs.name
    includedEventTypes: [
      'Microsoft.AppConfiguration.KeyValueModified'
    ]
    topicName: configTopic.outputs.name
  }
  scope: resourceGroup(opsResourceGroupName)
  dependsOn: [eventgrid, configTopic]
}

@batchSize(3)
module srBackend 'modules/service.bicep' = [for service in items(backendServices): {
  name: 'srBackend-${service.key}-${timestamp}'
  params: {
    location: location
    namespace: k8sNamespace
    oidcIssuerUrl: aksBackend.outputs.oidcIssuerUrl
    opsResourceGroupName: opsResourceGroupName
    opsResourceSuffix: opsResourceSuffix
    resourceSuffix: resourceSuffix
    serviceName: service.key
    storageResourceGroupName: storageResourceGroupName
    tags: tags
  }
}]

@batchSize(3)
module srCoreApi 'modules/service.bicep' = [for service in items(coreApiService): {
  name: 'srCoreApi-${service.key}-${timestamp}'
  params: {
    clientSecret: coreApiClientSecret
    location: location
    namespace: k8sNamespace
    oidcIssuerUrl: aksBackend.outputs.oidcIssuerUrl
    opsResourceGroupName: opsResourceGroupName
    opsResourceSuffix: opsResourceSuffix
    resourceSuffix: resourceSuffix
    serviceName: service.key
    storageResourceGroupName: storageResourceGroupName
    tags: tags
    useOidc: true
  }
}]

@batchSize(3)
module srChatUi 'modules/service.bicep' = [for service in items(chatUiService): {
  name: 'srChatUi-${service.key}-${timestamp}'
  params: {
    clientSecret: chatUiClientSecret
    location: location
    namespace: k8sNamespace
    oidcIssuerUrl: aksFrontend.outputs.oidcIssuerUrl
    opsResourceGroupName: opsResourceGroupName
    opsResourceSuffix: opsResourceSuffix
    resourceSuffix: resourceSuffix
    serviceName: service.key
    storageResourceGroupName: storageResourceGroupName
    tags: tags
    useOidc: true
  }
}]

@batchSize(3)
module srManagementApi 'modules/service.bicep' = [for service in items(managementApiService): {
  name: 'srManagementApi-${service.key}-${timestamp}'
  params: {
    clientSecret: managementApiClientSecret
    location: location
    namespace: k8sNamespace
    oidcIssuerUrl: aksBackend.outputs.oidcIssuerUrl
    opsResourceGroupName: opsResourceGroupName
    opsResourceSuffix: opsResourceSuffix
    resourceSuffix: resourceSuffix
    serviceName: service.key
    storageResourceGroupName: storageResourceGroupName
    tags: tags
    useOidc: true
  }
}]

@batchSize(3)
module srManagementUi 'modules/service.bicep' = [for service in items(managementUiService): {
  name: 'srManagementUi-${service.key}-${timestamp}'
  params: {
    clientSecret: managementUiClientSecret
    location: location
    namespace: k8sNamespace
    oidcIssuerUrl: aksFrontend.outputs.oidcIssuerUrl
    opsResourceGroupName: opsResourceGroupName
    opsResourceSuffix: opsResourceSuffix
    resourceSuffix: resourceSuffix
    serviceName: service.key
    storageResourceGroupName: storageResourceGroupName
    tags: tags
    useOidc: true
  }
}]

@batchSize(3)
module srVectorizationApi 'modules/service.bicep' = [for service in items(vectorizationApiService): {
  name: 'srVectorizationApi-${service.key}-${timestamp}'
  params: {
    clientSecret: vectorizationApiClientSecret
    location: location
    namespace: k8sNamespace
    oidcIssuerUrl: aksBackend.outputs.oidcIssuerUrl
    opsResourceGroupName: opsResourceGroupName
    opsResourceSuffix: opsResourceSuffix
    resourceSuffix: resourceSuffix
    serviceName: service.key
    storageResourceGroupName: storageResourceGroupName
    tags: tags
    useOidc: false
  }
}]

module coreApiosmosRoles './modules/sqlRoleAssignments.bicep' = {
  scope: resourceGroup(storageResourceGroupName)
  name: 'core-api-cosmos-role'
  params: {
    accountName: cosmosDb.name
    principalId: srCoreApi[0].outputs.servicePrincipalId
    roleDefinitionIds: {
      'Cosmos DB Built-in Data Contributor': '00000000-0000-0000-0000-000000000002'
    }
  }
}

module cosmosRoles './modules/sqlRoleAssignments.bicep' = {
  scope: resourceGroup(storageResourceGroupName)
  name: 'core-job-cosmos-role'
  params: {
    accountName: cosmosDb.name
    principalId: srBackend[indexOf(backendServiceNames, 'core-job')].outputs.servicePrincipalId
    roleDefinitionIds: {
      'Cosmos DB Built-in Data Contributor': '00000000-0000-0000-0000-000000000002'
    }
  }
}

module searchIndexDataReaderRole 'modules/utility/roleAssignments.bicep' = {
  name: 'searchIndexDataRole-${timestamp}'
  scope: resourceGroup(vectorizationResourceGroupName)
  params: {
    principalId: srVectorizationApi[indexOf(vecServiceNames, 'vectorization-api')].outputs.servicePrincipalId
    roleDefinitionIds: {
      'Search Index Data Contributor': '8ebe5a00-799e-43f5-93ac-243d3dce84a7'
      'Search Service Contributor': '7ca78c08-252a-4471-8644-bb5ff32d4ba0'
    }
  }
}

module searchIndexDataReaderWorkerRole 'modules/utility/roleAssignments.bicep' = {
  name: 'searchIndexDataWorkerRole-${timestamp}'
  scope: resourceGroup(vectorizationResourceGroupName)
  params: {
    principalId: srBackend[indexOf(backendServiceNames, 'vectorization-job')].outputs.servicePrincipalId
    roleDefinitionIds: {
      'Search Index Data Contributor': '8ebe5a00-799e-43f5-93ac-243d3dce84a7'
      'Search Service Contributor': '7ca78c08-252a-4471-8644-bb5ff32d4ba0'
    }
  }
}

module cognitiveServicesOpenAiUserRole 'modules/utility/roleAssignments.bicep' = {
  name: 'cognitiveServicesOpenAiUserRole-${timestamp}'
  scope: resourceGroup(openAiResourceGroupName)
  params: {
    principalId: srBackend[indexOf(vecServiceNames, 'vectorization-api')].outputs.servicePrincipalId
    roleDefinitionIds: {
      'Cognitive Services OpenAI User': '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
    }
  }
}

module cognitiveServicesOpenAiUserWorkerRole 'modules/utility/roleAssignments.bicep' = {
  name: 'cognitiveServicesOpenAiUserWorkerRole-${timestamp}'
  scope: resourceGroup(openAiResourceGroupName)
  params: {
    principalId: srBackend[indexOf(backendServiceNames, 'vectorization-job')].outputs.servicePrincipalId
    roleDefinitionIds: {
      'Cognitive Services OpenAI User': '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
    }
  }
}

module cognitiveServicesOpenAiUserGatewayRole 'modules/utility/roleAssignments.bicep' = {
  name: 'cognitiveServicesOpenAiUserGatewayRole-${timestamp}'
  scope: resourceGroup(openAiResourceGroupName)
  params: {
    principalId: srBackend[indexOf(backendServiceNames, 'gateway-api')].outputs.servicePrincipalId
    roleDefinitionIds: {
      'Cognitive Services OpenAI User': '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
    }
  }
}

module cognitiveServicesOpenAiUserLangChainRole 'modules/utility/roleAssignments.bicep' = {
  name: 'cognitiveServicesOpenAiUserLangChainRole-${timestamp}'
  scope: resourceGroup(openAiResourceGroupName)
  params: {
    principalId: srBackend[indexOf(backendServiceNames, 'langchain-api')].outputs.servicePrincipalId
    roleDefinitionIds: {
      'Cognitive Services OpenAI User': '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
    }
  }
}

module cognitiveServicesOpenAiUserSemanticKernelRole 'modules/utility/roleAssignments.bicep' = {
  name: 'cognitiveServicesOpenAiUserSemKernelRole-${timestamp}'
  scope: resourceGroup(openAiResourceGroupName)
  params: {
    principalId: srBackend[indexOf(backendServiceNames, 'semantic-kernel-api')].outputs.servicePrincipalId
    roleDefinitionIds: {
      'Cognitive Services OpenAI User': '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
    }
  }
}

module searchIndexDataReaderLangchainRole 'modules/utility/roleAssignments.bicep' = {
  name: 'searchIndexDataReaderLangchainRole-${timestamp}'
  scope: resourceGroup(vectorizationResourceGroupName)
  params: {
    principalId: srBackend[indexOf(backendServiceNames, 'langchain-api')].outputs.servicePrincipalId
    roleDefinitionIds: {
      'Search Index Data Reader': '1407120a-92aa-4202-b7e9-c0e197c71c8f'
    }
  }
}

module searchIndexDataReaderSemanticKernelRole 'modules/utility/roleAssignments.bicep' = {
  name: 'searchIndexDataReaderSemKerRole-${timestamp}'
  scope: resourceGroup(vectorizationResourceGroupName)
  params: {
    principalId: srBackend[indexOf(backendServiceNames, 'semantic-kernel-api')].outputs.servicePrincipalId
    roleDefinitionIds: {
      'Search Index Data Reader': '1407120a-92aa-4202-b7e9-c0e197c71c8f'
    }
  }
}
