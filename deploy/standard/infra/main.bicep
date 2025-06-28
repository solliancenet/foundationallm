targetScope = 'subscription'

param administratorObjectId string
param allowedExternalCidr string
param aksServiceCidr string
param authAppRegistrationClientId string
param authAppRegistrationInstance string
param authAppRegistrationTenantId string
param backendAksNodeSku string
param backendAksSystemNodeSku string
param frontendAksNodeSku string
param frontendAksSystemNodeSku string
param vmAvailabilityZones string
param cidrVnet string
param createDate string = utcNow('u')
param deploymentOwner string
param environmentName string
param existingOpenAiInstanceName string = ''
param existingOpenAiInstanceRg string = ''
param existingOpenAiInstanceSub string = ''
param instanceId string
param location string
param networkName string = ''
param oneDriveBaseUrl string
param principalType string
param project string
param registry string
param services array
param timestamp string = utcNow()
param userPortalHostname string
param managementPortalHostname string
param coreApiHostname string
param managementApiHostname string

param dnsResourceGroup string
param dnsSubscriptionId string = subscription().subscriptionId

param hubResourceGroup string
param hubSubscriptionId string = subscription().subscriptionId
param hubVnetName string
param resourceGroups object

// Locals
var k8sNamespace = 'fllm'
var availabilityZones = split(vmAvailabilityZones, ',')

var existingOpenAiInstance = {
  name: existingOpenAiInstanceName
  resourceGroup: existingOpenAiInstanceRg
  subscriptionId: existingOpenAiInstanceSub
}

var tags = {
  'compute-type': 'aks'
  'create-date': createDate
  'azd-env-name': environmentName
  'iac-type': 'bicep'
  'project-name': project
  'owner': deploymentOwner
}

// Functions
func namer(resourceAbbr string, env string, region string, workloadName string, projectId string) string =>
  '${resourceAbbr}${env}-${region}-${workloadName}-${projectId}'

// Resources
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = [
  for rgName in items(resourceGroups): {
    name: rgName.value
    location: location
    tags: union(tags, {'deployment': split(split(userPortalHostname, '.')[0], '-')[0]})
  }
]

// Nested Deployments
module app 'app-rg.bicep' = {
  dependsOn: [rg, networking, ops, storage]
  name: 'app-${timestamp}'
  scope: resourceGroup(resourceGroups.app)
  params: {
    actionGroupId: ops.outputs.actionGroupId
    administratorObjectId: administratorObjectId
    aksServiceCidr: aksServiceCidr
    availabilityZones: availabilityZones
    backendAksNodeSku: backendAksNodeSku
    backendAksSystemNodeSku: backendAksSystemNodeSku
    contextResourceGroupName: resourceGroups.context
    frontendAksNodeSku: frontendAksNodeSku
    frontendAksSystemNodeSku: frontendAksSystemNodeSku
    environmentName: environmentName
    dnsResourceGroup: dnsResourceGroup
    dnsSubscriptionId: dnsSubscriptionId
    k8sNamespace: k8sNamespace
    location: location
    logAnalyticsWorkspaceId: ops.outputs.logAnalyticsWorkspaceId
    logAnalyticsWorkspaceResourceId: ops.outputs.logAnalyticsWorkspaceId
    monitorWorkspaceName: ops.outputs.monitorWorkspaceName
    networkingResourceGroupName: resourceGroups.net
    openAiResourceGroupName: resourceGroups.oai
    openAiName: openai.outputs.azureOpenAiName
    deployOpenAi: empty(existingOpenAiInstance.name)
    opsResourceGroupName: resourceGroups.ops
    project: project
    services: services
    storageResourceGroupName: resourceGroups.storage
    vectorizationResourceGroupName: resourceGroups.vec
    vnetName: networking.outputs.vnetName
  }
}

module auth 'auth-rg.bicep' = {
  dependsOn: [rg, app]
  name: 'auth-${timestamp}'
  scope: resourceGroup(resourceGroups.auth)
  params: {
    actionGroupId: ops.outputs.actionGroupId
    administratorObjectId: administratorObjectId
    appResourceGroupName: resourceGroups.app
    authAppRegistrationClientId: authAppRegistrationClientId
    authAppRegistrationInstance: authAppRegistrationInstance
    authAppRegistrationTenantId: authAppRegistrationTenantId
    environmentName: environmentName
    dnsResourceGroup: dnsResourceGroup
    dnsSubscriptionId: dnsSubscriptionId
    instanceId: instanceId
    k8sNamespace: k8sNamespace
    location: location
    logAnalyticsWorkspaceId: ops.outputs.logAnalyticsWorkspaceId
    opsResourceGroupName: resourceGroups.ops
    principalType: principalType
    project: project
    vnetId: networking.outputs.vnetId
  }
}

module context 'context-rg.bicep' = {
  dependsOn: [rg, app]
  name: 'context-${timestamp}'
  scope: resourceGroup(resourceGroups.context)
  params: {
    actionGroupId: ops.outputs.actionGroupId
    administratorObjectId: administratorObjectId
    appResourceGroupName: resourceGroups.app
    environmentName: environmentName
    dnsResourceGroup: dnsResourceGroup
    dnsSubscriptionId: dnsSubscriptionId
    instanceId: instanceId
    k8sNamespace: k8sNamespace
    location: location
    logAnalyticsWorkspaceId: ops.outputs.logAnalyticsWorkspaceId
    opsResourceGroupName: resourceGroups.ops
    storageResourceGroupName: resourceGroups.storage
    principalType: principalType
    project: project
    services: services
    vnetId: networking.outputs.vnetId
  }
}


module networking 'networking-rg.bicep' = {
  dependsOn: [rg]
  name: 'networking-${timestamp}'
  scope: resourceGroup(resourceGroups.net)
  params: {
    cidrVnet: cidrVnet
    allowedExternalCidr: allowedExternalCidr
    environmentName: environmentName
    dnsResourceGroup: dnsResourceGroup
    dnsSubscriptionId: dnsSubscriptionId
    hubResourceGroup: hubResourceGroup
    hubSubscriptionId: hubSubscriptionId
    hubVnetName: hubVnetName
    location: location
    networkName: networkName
    project: project
  }
}

module openai 'openai-rg.bicep' = {
  dependsOn: [rg, networking]
  name: 'openai-${timestamp}'
  scope: resourceGroup(resourceGroups.oai)
  params: {
    actionGroupId: ops.outputs.actionGroupId
    dnsResourceGroup: dnsResourceGroup
    dnsSubscriptionId: dnsSubscriptionId
    environmentName: environmentName
    existingOpenAiInstance: existingOpenAiInstance
    location: location
    logAnalyticsWorkspaceId: ops.outputs.logAnalyticsWorkspaceId
    opsKeyVaultName: ops.outputs.keyVaultName
    opsResourceGroupName: resourceGroups.ops
    project: project
    vnetId: networking.outputs.vnetId
  }
}

module ops 'ops-rg.bicep' = {
  dependsOn: [rg, networking]
  name: 'ops-${timestamp}'
  scope: resourceGroup(resourceGroups.ops)
  params: {
    administratorObjectId: administratorObjectId
    dnsResourceGroup: dnsResourceGroup
    dnsSubscriptionId: dnsSubscriptionId
    environmentName: environmentName
    location: location
    project: project
    vnetId: networking.outputs.vnetId
  }
}

module storage 'storage-rg.bicep' = {
  dependsOn: [rg, networking]
  name: 'storage-${timestamp}'
  scope: resourceGroup(resourceGroups.storage)
  params: {
    actionGroupId: ops.outputs.actionGroupId
    administratorObjectId: administratorObjectId
    dnsResourceGroup: dnsResourceGroup
    dnsSubscriptionId: dnsSubscriptionId
    environmentName: environmentName
    location: location
    logAnalyticsWorkspaceId: ops.outputs.logAnalyticsWorkspaceId
    principalType: principalType
    project: project
    vnetId: networking.outputs.vnetId
  }
}

module vec 'vec-rg.bicep' = {
  dependsOn: [rg, networking]
  name: 'vec-${timestamp}'
  scope: resourceGroup(resourceGroups.vec)
  params: {
    actionGroupId: ops.outputs.actionGroupId
    dnsResourceGroup: dnsResourceGroup
    dnsSubscriptionId: dnsSubscriptionId
    environmentName: environmentName
    location: location
    logAnalyticsWorkspaceId: ops.outputs.logAnalyticsWorkspaceId
    project: project
    vnetId: networking.outputs.vnetId
  }
}

output ADMIN_GROUP_OBJECT_ID string = administratorObjectId

output AZURE_CONTENT_SAFETY_ENDPOINT string = openai.outputs.azureContentSafetyEndpoint
output AZURE_OPENAI_ENDPOINT string = openai.outputs.azureOpenAiEndpoint
output AZURE_OPENAI_ID string = openai.outputs.azureOpenAiId
output AZURE_STORAGE_ACCOUNT_NAME string = storage.outputs.storageAccountName

output CONTEXT_STORAGE_ACCOUNT_NAME string = context.outputs.contextStoreName
output CONTEXT_SESSION_POOL_ENDPOINT string = context.outputs.sessionPoolEndpoint

output FOUNDATIONALLM_PROJECT string = project
output FOUNDATIONALLM_K8S_NS string = k8sNamespace
output FOUNDATIONALLM_REGISTRY string = registry

output FLLM_APP_RG     string = resourceGroups.app
output FLLM_AUTH_RG    string = resourceGroups.auth
output FLLM_CONTEXT_RG string = resourceGroups.context
output FLLM_DATA_RG    string = resourceGroups.data
output FLLM_JBX_RG     string = resourceGroups.jbx
output FLLM_NET_RG     string = resourceGroups.net
output FLLM_OAI_RG     string = resourceGroups.oai
output FLLM_OPS_RG     string = resourceGroups.ops
output FLLM_STORAGE_RG string = resourceGroups.storage
output FLLM_VEC_RG     string = resourceGroups.vec

output FLLM_OPS_KV string = ops.outputs.keyVaultName

output FLLM_CHAT_PORTAL_HOSTNAME string = userPortalHostname
output FLLM_MGMT_PORTAL_HOSTNAME string = managementPortalHostname
output FLLM_CORE_API_HOSTNAME string = coreApiHostname
output FLLM_MGMT_API_HOSTNAME string = managementApiHostname

output FOUNDATIONALLM_VNET_NAME string = networking.outputs.vnetName
output FOUNDATIONALLM_VNET_ID string = networking.outputs.vnetId
output FOUNDATIONALLM_HUB_VNET_ID string = networking.outputs.hubVnetId

output ONEDRIVE_BASE_URL string = oneDriveBaseUrl

output SERVICE_GATEKEEPER_API_ENDPOINT_URL string = 'http://gatekeeper-api/gatekeeper/'
output SERVICE_GATEKEEPER_INTEGRATION_API_ENDPOINT_URL string = 'http://gatekeeper-integration-api/gatekeeperintegration'
output SERVICE_GATEWAY_ADAPTER_API_ENDPOINT_URL string = 'http://gateway-adapter-api/gatewayadapter'
output SERVICE_GATEWAY_API_ENDPOINT_URL string = 'http://gateway-api/gateway'
output SERVICE_LANGCHAIN_API_ENDPOINT_URL string = 'http://langchain-api/langchain'
output SERVICE_ORCHESTRATION_API_ENDPOINT_URL string = 'http://orchestration-api/orchestration'
output SERVICE_SEMANTIC_KERNEL_API_ENDPOINT_URL string = 'http://semantic-kernel-api/semantickernel'
output SERVICE_STATE_API_ENDPOINT_URL string = 'http://state-api/state'
output SERVICE_VECTORIZATION_API_ENDPOINT_URL string = 'http://vectorization-api/vectorization'
output SERVICE_VECTORIZATION_JOB_ENDPOINT_URL string = 'http://vectorization-job/vectorization'
output SERVICE_CHAT_UI_ENDPOINT_URL string = 'https://${userPortalHostname}'
output SERVICE_CORE_API_ENDPOINT_URL string = 'https://${coreApiHostname}'
output SERVICE_MANAGEMENT_API_ENDPOINT_URL string = 'https://${managementApiHostname}'
output SERVICE_MANAGEMENT_UI_ENDPOINT_URL string = 'https://${managementPortalHostname}'
output SERVICE_CONTEXT_API_ENDPOINT_URL string = 'http://context-api/context'
output SERVICE_DATAPIPELINE_API_ENDPOINT_URL string = 'http://datapipeline-api/datapipeline'
output SERVICE_DATAPIPELINE_FRONTENDWORKER_ENDPOINT_URL string = 'http://datapipeline-frontendworker/datapipelinefrontendworker'
output SERVICE_DATAPIPELINE_BACKENDWORKER_ENDPOINT_URL string = 'http://datapipeline-backendworker/datapipelinebackendworker'
