targetScope = 'subscription'

param adminGroupObjectId string

param authAppRegistration object
param timestamp string = utcNow()
param appRegistrations array
param isE2ETest bool = false

param createDate string = utcNow('u')

@maxLength(32)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

param instanceId string = guid(subscription().id, location, environmentName)

@description('Primary location for all resources')
param location string

param existingOpenAiInstance object

@description('Id of the user or app to assign application roles')
param principalId string

param principalType string = 'User'

@secure()
param serviceDefinition object

param authService object
param services array

param authServiceExists bool
param servicesExist object

/********** Locals **********/
var clientSecrets = [
  {
    name: 'foundationallm-apis-chat-ui-entra-clientsecret'
    value: 'PLACEHOLDER'
  }
  {
    name: 'foundationallm-apis-core-api-entra-clientsecret'
    value: 'PLACEHOLDER'
  }
  {
    name: 'foundationallm-apis-management-api-entra-clientsecret'
    value: 'PLACEHOLDER'
  }
  {
    name: 'foundationallm-apis-management-ui-entra-clientsecret'
    value: 'PLACEHOLDER'
  }
  {
    name: 'foundationallm-apis-vectorization-api-entra-clientsecret'
    value: 'PLACEHOLDER'
  }
  {
    name: 'foundationallm-langchain-csvfile-url'
    value: 'PLACEHOLDER'
  }
  {
    name: 'foundationallm-langchain-sqldatabase-testdb-password'
    value: 'PLACEHOLDER'
  }
]

var deployOpenAi = empty(existingOpenAiInstance.name)
var azureOpenAiEndpoint = deployOpenAi ? openAi.outputs.endpoint : customerOpenAi.properties.endpoint
var azureOpenAiId = deployOpenAi ? openAi.outputs.id : customerOpenAi.id
var azureOpenAi = deployOpenAi ? openAiInstance : existingOpenAiInstance
var openAiInstance = {
  name: openAi.outputs.name
  resourceGroup: rg.name
  subscriptionId: subscription().subscriptionId
}

// Tags that should be applied to all resources.
//
// Note that 'azd-service-name' tags should be applied separately to service host resources.
// Example usage:
//   tags: union(tags, { 'azd-service-name': <service name in azure.yaml> })
var tags = {
  'azd-env-name': environmentName
  'compute-type': 'container-app'
  'create-date': createDate
}

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))

/********** Resources **********/
resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

resource authRg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-auth-${environmentName}'
  location: location
  tags: tags
}

resource customerOpenAiResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing =
  if (!deployOpenAi) {
    scope: subscription(existingOpenAiInstance.subscriptionId)
    name: existingOpenAiInstance.resourceGroup
  }

resource customerOpenAi 'Microsoft.CognitiveServices/accounts@2023-05-01' existing =
  if (!deployOpenAi) {
    name: existingOpenAiInstance.name
    scope: customerOpenAiResourceGroup
  }

/********** Nested Modules **********/
module appConfig './shared/app-config.bicep' = {
  name: 'app-config-${timestamp}'
  params: {
    keyvaultName: keyVault.outputs.name
    location: location
    name: '${abbrs.appConfigurationConfigurationStores}${resourceToken}'
    services: services
    sku: 'standard'
    tags: tags
  }
  scope: rg
  dependsOn: [keyVault]
}

module authKeyvault './shared/keyvault.bicep' = {
  name: 'auth-kv-${timestamp}'
  params: {
    location: location
    name: '${abbrs.keyVaultVaults}auth${resourceToken}'
    tags: tags
    principalId: principalId
    principalType: principalType
    secrets: [
      {
        name: 'foundationallm-authorizationapi-entra-instance'
        value: authAppRegistration.instance
      }
      {
        name: 'foundationallm-authorizationapi-entra-tenantid'
        value: authAppRegistration.tenantId
      }
      {
        name: 'foundationallm-authorizationapi-entra-clientid'
        value: authAppRegistration.clientId
      }
      {
        name: 'foundationallm-authorizationapi-storage-accountname'
        value: authStore.outputs.name
      }
      {
        name: 'foundationallm-authorizationapi-instanceids'
        value: instanceId
      }
      {
        name: 'foundationallm-authorizationapi-appinsights-connectionstring'
        value: monitoring.outputs.applicationInsightsConnectionString
      }
    ]
  }
  scope: authRg
}

module authStore './shared/authorization-store.bicep' = {
  name: 'auth-store-${timestamp}'
  params: {
    adminGroupObjectId: adminGroupObjectId
    location: location
    name: '${abbrs.storageStorageAccounts}auth${resourceToken}'
    tags: tags
  }
  scope: authRg
}

module contentSafety './shared/content-safety.bicep' = {
  name: 'content-safety-${timestamp}'
  params: {
    keyvaultName: keyVault.outputs.name
    location: location
    name: '${abbrs.cognitiveServicesAccounts}${resourceToken}'
    sku: 'S0'
    tags: tags
  }
  scope: rg
  dependsOn: [keyVault]
}

module cosmosDb './shared/cosmosdb.bicep' = {
  name: 'cosmos-${timestamp}'
  params: {
    containers: [
      {
        name: 'UserSessions'
        partitionKeyPath: '/upn'
        maxThroughput: 1000
      }
      {
        name: 'UserProfiles'
        partitionKeyPath: '/upn'
        maxThroughput: 1000
      }
      {
        name: 'Sessions'
        partitionKeyPath: '/sessionId'
        maxThroughput: 1000
      }
      {
        name: 'leases'
        partitionKeyPath: '/id'
        maxThroughput: 1000
      }
    ]
    databaseName: 'database'
    location: location
    name: '${abbrs.documentDBDatabaseAccounts}${resourceToken}'
    tags: tags
  }
  scope: rg
  dependsOn: [keyVault]
}

module cogSearch './shared/search.bicep' = {
  name: 'cogsearch-${timestamp}'
  params: {
    location: location
    name: '${abbrs.searchSearchServices}${resourceToken}'
    sku: 'basic'
    tags: tags
  }
  scope: rg
}

var searchReaderRoleTargets = [
  'langchain-api'
  'semantic-kernel-api'
]

var searchWriterRoleTargets = [
  'vectorization-api'
  'vectorization-job'
]

var openAiRoleTargets = [
  'gateway-api'
  'semantic-kernel-api'
  'langchain-api'
]

module searchReaderRoles './shared/roleAssignments.bicep' = [
  for target in searchReaderRoleTargets: {
    scope: rg
    name: '${target}-search-reader-role-${timestamp}'
    params: {
      principalId: acaServices[indexOf(serviceNames, target)].outputs.miPrincipalId
      roleDefinitionIds: {
        'Search Index Data Reader': '1407120a-92aa-4202-b7e9-c0e197c71c8f'
      }
    }
  }
]

module searchWriterRoles './shared/roleAssignments.bicep' = [
  for target in searchWriterRoleTargets: {
    scope: rg
    name: '${target}-search-contrib-role-${timestamp}'
    params: {
      principalId: acaServices[indexOf(serviceNames, target)].outputs.miPrincipalId
      roleDefinitionIds: {
        'Search Service Contributor': '7ca78c08-252a-4471-8644-bb5ff32d4ba0'
        'Search Index Data Contributor': '8ebe5a00-799e-43f5-93ac-243d3dce84a7'
      }
    }
  }
]

module dashboard './shared/dashboard-web.bicep' = {
  name: 'dashboard-${timestamp}'
  params: {
    name: '${abbrs.portalDashboards}${resourceToken}'
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    location: location
    tags: tags
  }
  scope: rg
}

module eventgrid './shared/eventgrid.bicep' = {
  name: 'eventgrid-${timestamp}'
  params: {
    name: '${abbrs.eventGridDomains}${resourceToken}'
    location: location
    tags: tags
    keyvaultName: keyVault.outputs.name
    topics: [
      {
        name: 'storage'
      }
      {
        name: 'vectorization'
      }
      {
        name: 'configuration'
      }
    ]
  }
  scope: rg
}

module keyVault './shared/keyvault.bicep' = {
  name: 'keyvault-${timestamp}'
  params: {
    location: location
    tags: tags
    name: '${abbrs.keyVaultVaults}${resourceToken}'
    principalId: principalId
    principalType: principalType
    secrets: clientSecrets
  }
  scope: rg
}

module monitoring './shared/monitoring.bicep' = {
  name: 'monitoring-${timestamp}'
  params: {
    keyvaultName: keyVault.outputs.name
    location: location
    tags: tags
    logAnalyticsName: '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsName: '${abbrs.insightsComponents}${resourceToken}'
  }
  scope: rg
  dependsOn: [keyVault]
}

module openAi './shared/openai.bicep' =
  if (deployOpenAi) {
    dependsOn: [keyVault]
    name: 'openai-${timestamp}'
    scope: rg

    params: {
      location: location
      name: '${abbrs.openAiAccounts}${resourceToken}'
      sku: 'S0'
      tags: tags

      deployments: [
        {
          name: 'completions'
          sku: {
            name: 'Standard'
            capacity: 10
          }
          model: {
            name: 'gpt-35-turbo'
            version: '0613'
          }
        }
        {
          name: 'embeddings'
          sku: {
            name: 'Standard'
            capacity: 10
          }
          model: {
            name: 'text-embedding-ada-002'
            version: '2'
          }
        }
      ]
    }
  }

module openAiSecrets './shared/openai-secrets.bicep' = {
  name: 'openaiSecrets-${timestamp}'
  scope: rg

  params: {
    keyvaultName: keyVault.outputs.name
    openAiInstance: azureOpenAi
    tags: tags
  }
}

module storage './shared/storage.bicep' = {
  name: 'storage-${timestamp}'
  params: {
    containers: [
      {
        name: 'agents'
      }
      {
        name: 'data-sources'
      }
      {
        name: 'foundationallm-source'
      }
      {
        name: 'prompts'
      }
      {
        name: 'resource-provider'
      }
      {
        name: 'vectorization-input'
      }
      {
        name: 'vectorization-state'
      }
    ]
    files: []
    queues: [
      {
        name: 'extract'
      }
      {
        name: 'embed'
      }
      {
        name: 'partition'
      }
      {
        name: 'index'
      }
    ]
    location: location
    name: '${abbrs.storageStorageAccounts}${resourceToken}'
    tags: tags
  }
  scope: rg
  dependsOn: [keyVault]
}

module configTopic 'shared/config-system-topic.bicep' = {
  name: 'configTopic-${timestamp}'
  params: {
    name: '${abbrs.eventGridDomainsTopics}config${resourceToken}'
    eventGridName: eventgrid.outputs.name
    destinationTopicName: 'configuration'
    location: location
    tags: tags
    appConfigAccountName: appConfig.outputs.name
  }
  scope: rg
  dependsOn: [eventgrid]
}

module storageTopic 'shared/storage-system-topic.bicep' = {
  name: 'storageTopic-${timestamp}'
  params: {
    name: '${abbrs.eventGridDomainsTopics}storage${resourceToken}'
    eventGridName: eventgrid.outputs.name
    destinationTopicName: 'storage'
    location: location
    tags: tags
    storageAccountName: storage.outputs.name
  }
  scope: rg
  dependsOn: [eventgrid]
}

module storageSub 'shared/system-topic-subscription.bicep' = {
  name: 'storageSub-${timestamp}'
  params: {
    name: 'foundationallm-storage'
    eventGridName: eventgrid.outputs.name
    topicName: storageTopic.outputs.name
    destinationTopicName: 'storage'
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
  }
  scope: rg
  dependsOn: [eventgrid, storageTopic]
}

module configSub 'shared/system-topic-subscription.bicep' = {
  name: 'configSub-${timestamp}'
  params: {
    name: 'app-config'
    eventGridName: eventgrid.outputs.name
    topicName: configTopic.outputs.name
    destinationTopicName: 'configuration'
    includedEventTypes: [
      'Microsoft.AppConfiguration.KeyValueModified'
    ]
  }
  scope: rg
  dependsOn: [eventgrid, configTopic]
}

module appsEnv './shared/apps-env.bicep' = {
  name: 'apps-env-${timestamp}'
  params: {
    name: '${abbrs.appManagedEnvironments}${resourceToken}'
    location: location
    tags: tags
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    logAnalyticsWorkspaceName: monitoring.outputs.logAnalyticsWorkspaceName
  }
  scope: rg
}

module authAcaService './app/authAcaService.bicep' = {
  name: '${authService.name}-${timestamp}'
  params: {
    name: '${abbrs.appContainerApps}authapi${resourceToken}'
    location: location
    tags: tags
    authRgName: authRg.name
    authStoreName: authStore.outputs.name
    identityName: '${abbrs.managedIdentityUserAssignedIdentities}auth-api-${resourceToken}'
    keyvaultName: authKeyvault.outputs.name
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    containerAppsEnvironmentName: appsEnv.outputs.name
    cpu: authService.cpu
    memory: authService.memory
    replicaCount: empty(authService.replicaCount) ? 0 : int(authService.replicaCount)
    exists: authServiceExists == 'true'
    appDefinition: serviceDefinition
    hasIngress: true
    imageName: authService.image
    envSettings: [
      {
        name: 'FoundationaLLM_AuthorizationAPI_KeyVaultURI'
        value: authKeyvault.outputs.endpoint
      }
    ]
    serviceName: 'auth-api'
  }
  scope: rg
  dependsOn: [authStore, keyVault, monitoring]
}

@batchSize(3)
module acaServices './app/acaService.bicep' = [
  for service in services: {
    dependsOn: [appConfig, cogSearch, contentSafety, cosmosDb, keyVault, monitoring, storage]
    name: '${service.name}-${timestamp}'
    scope: rg
    params: {
      apiKeySecretName: service.apiKeySecretName
      appDefinition: serviceDefinition
      applicationInsightsName: monitoring.outputs.applicationInsightsName
      containerAppsEnvironmentName: appsEnv.outputs.name
      cpu: service.cpu
      exists: servicesExist['${service.name}'] == 'true'
      hasIngress: service.hasIngress
      identityName: '${abbrs.managedIdentityUserAssignedIdentities}${service.name}-${resourceToken}'
      imageName: service.image
      keyvaultName: keyVault.outputs.name
      location: location
      memory: service.memory
      name: '${abbrs.appContainerApps}${service.name}'
      replicaCount: empty(service.replicaCount) ? 0 : int(service.replicaCount)
      resourceToken: resourceToken
      serviceName: service.name
      tags: tags

      envSettings: union(service.useEndpoint
        ? [
            {
              name: service.appConfigEnvironmentVarName
              value: appConfig.outputs.endpoint
            }
          ]
        : [], isE2ETest
        ? [
            {
              name: 'FOUNDATIONALLM_ENVIRONMENT'
              value: 'E2ETest'
            }
          ]
        : [])

      secretSettings: service.useEndpoint
        ? []
        : [
            {
              name: service.appConfigEnvironmentVarName
              value: filter(appConfig.outputs.connectionStringSecret, item => item.name == service.name)[0].uri
              secretRef: 'appconfig-connection-string'
            }
          ]
    }
  }
]

var cosmosRoleTargets = [
  'core-api'
  'core-job'
]

module cosmosRoles './shared/sqlRoleAssignments.bicep' = [
  for target in cosmosRoleTargets: {
    scope: rg
    name: '${target}-cosmos-role-${timestamp}'
    params: {
      accountName: cosmosDb.outputs.name
      principalId: acaServices[indexOf(serviceNames, target)].outputs.miPrincipalId
      roleDefinitionIds: {
        'Cosmos DB Built-in Data Contributor': '00000000-0000-0000-0000-000000000002'
      }
    }
  }
]

module openAiRoles './shared/roleAssignments.bicep' = [
  for target in openAiRoleTargets: {
    scope: rg
    name: '${target}-openai-roles-${timestamp}'
    params: {
      principalId: acaServices[indexOf(serviceNames, target)].outputs.miPrincipalId
      roleDefinitionNames: [
        'Cognitive Services OpenAI User'
        'Reader'
      ]
    }
  }
]

var contentSafetyTargets = [
  'gatekeeper-api'
]

module contentSafetyRoles './shared/roleAssignments.bicep' = [
  for target in contentSafetyTargets: {
    scope: rg
    name: '${target}-cs-roles-${timestamp}'
    params: {
      principalId: acaServices[indexOf(serviceNames, target)].outputs.miPrincipalId
      roleDefinitionNames: [
        'Cognitive Services User'
      ]
    }
  }
]

output AZURE_APP_CONFIG_NAME string = appConfig.outputs.name
output AZURE_AUTHORIZATION_STORAGE_ACCOUNT_NAME string = authStore.outputs.name
output AZURE_COGNITIVE_SEARCH_ENDPOINT string = cogSearch.outputs.endpoint
output AZURE_COGNITIVE_SEARCH_NAME string = cogSearch.outputs.name
output AZURE_CONTENT_SAFETY_ENDPOINT string = contentSafety.outputs.endpoint
output AZURE_COSMOS_DB_ENDPOINT string = cosmosDb.outputs.endpoint
output AZURE_COSMOS_DB_NAME string = cosmosDb.outputs.name
output AZURE_EVENT_GRID_ENDPOINT string = eventgrid.outputs.endpoint
output AZURE_EVENT_GRID_ID string = eventgrid.outputs.id
output AZURE_KEY_VAULT_ENDPOINT string = keyVault.outputs.endpoint
output AZURE_KEY_VAULT_NAME string = keyVault.outputs.name
output AZURE_OPENAI_ENDPOINT string = azureOpenAiEndpoint
output AZURE_OPENAI_ID string = azureOpenAiId
output AZURE_STORAGE_ACCOUNT_NAME string = storage.outputs.name

var appRegNames = [for appRegistration in appRegistrations: appRegistration.name]

output ENTRA_AUTH_API_SCOPES string = authAppRegistration.scopes

output ENTRA_CHAT_UI_CLIENT_ID string = appRegistrations[indexOf(appRegNames, 'chat-ui')].clientId
output ENTRA_CHAT_UI_SCOPES string = appRegistrations[indexOf(appRegNames, 'chat-ui')].scopes
output ENTRA_CHAT_UI_TENANT_ID string = appRegistrations[indexOf(appRegNames, 'chat-ui')].tenantId

output ENTRA_CORE_API_CLIENT_ID string = appRegistrations[indexOf(appRegNames, 'core-api')].clientId
output ENTRA_CORE_API_SCOPES string = appRegistrations[indexOf(appRegNames, 'core-api')].scopes
output ENTRA_CORE_API_TENANT_ID string = appRegistrations[indexOf(appRegNames, 'core-api')].tenantId

output ENTRA_MANAGEMENT_API_CLIENT_ID string = appRegistrations[indexOf(appRegNames, 'management-api')].clientId
output ENTRA_MANAGEMENT_API_SCOPES string = appRegistrations[indexOf(appRegNames, 'management-api')].scopes
output ENTRA_MANAGEMENT_API_TENANT_ID string = appRegistrations[indexOf(appRegNames, 'management-api')].tenantId

output ENTRA_MANAGEMENT_UI_CLIENT_ID string = appRegistrations[indexOf(appRegNames, 'management-ui')].clientId
output ENTRA_MANAGEMENT_UI_SCOPES string = appRegistrations[indexOf(appRegNames, 'management-ui')].scopes
output ENTRA_MANAGEMENT_UI_TENANT_ID string = appRegistrations[indexOf(appRegNames, 'management-ui')].tenantId

output ENTRA_VECTORIZATION_API_CLIENT_ID string = appRegistrations[indexOf(appRegNames, 'vectorization-api')].clientId
output ENTRA_VECTORIZATION_API_SCOPES string = appRegistrations[indexOf(appRegNames, 'vectorization-api')].scopes
output ENTRA_VECTORIZATION_API_TENANT_ID string = appRegistrations[indexOf(appRegNames, 'vectorization-api')].tenantId

output FOUNDATIONALLM_INSTANCE_ID string = instanceId

var serviceNames = [for service in services: service.name]

output RESOURCE_GROUP_NAME_DEFAULT string = rg.name
output SERVICE_ORCHESTRATION_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'orchestration-api')].outputs.uri
output SERVICE_ORCHESTRATION_API_MI_OBJECT_ID string = acaServices[indexOf(serviceNames, 'orchestration-api')].outputs.miPrincipalId
output SERVICE_AGENT_HUB_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'agent-hub-api')].outputs.uri
output SERVICE_AUTH_API_ENDPOINT_URL string = authAcaService.outputs.uri
output SERVICE_CHAT_UI_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'chat-ui')].outputs.uri
output SERVICE_CORE_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'core-api')].outputs.uri
output SERVICE_CORE_API_MI_OBJECT_ID string = acaServices[indexOf(serviceNames, 'core-api')].outputs.miPrincipalId
output SERVICE_CORE_JOB_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'core-job')].outputs.uri
output SERVICE_DATA_SOURCE_HUB_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'data-source-hub-api')].outputs.uri
output SERVICE_GATEKEEPER_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'gatekeeper-api')].outputs.uri
output SERVICE_GATEKEEPER_INTEGRATION_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'gatekeeper-integration-api')].outputs.uri
output SERVICE_GATEWAY_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'gateway-api')].outputs.uri
output SERVICE_LANGCHAIN_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'langchain-api')].outputs.uri
output SERVICE_MANAGEMENT_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'management-api')].outputs.uri
output SERVICE_MANAGEMENT_API_MI_OBJECT_ID string = acaServices[indexOf(serviceNames, 'management-api')].outputs.miPrincipalId
output SERVICE_MANAGEMENT_UI_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'management-ui')].outputs.uri
output SERVICE_PROMPT_HUB_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'prompt-hub-api')].outputs.uri
output SERVICE_SEMANTIC_KERNEL_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'semantic-kernel-api')].outputs.uri
output SERVICE_VECTORIZATION_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'vectorization-api')].outputs.uri
output SERVICE_VECTORIZATION_API_MI_OBJECT_ID string = acaServices[indexOf(serviceNames, 'vectorization-api')].outputs.miPrincipalId
output SERVICE_VECTORIZATION_JOB_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'vectorization-job')].outputs.uri
output SERVICE_VECTORIZATION_JOB_NAME string = acaServices[indexOf(serviceNames, 'vectorization-job')].outputs.name
