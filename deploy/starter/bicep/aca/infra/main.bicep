targetScope = 'subscription'

@description('Name of the the environment which is used to generate a short unique hash used in all resources.')
@maxLength(64)
@minLength(1)
param environmentName string

@description('Should the cognitive services account include a deployment for GPT-4?')
param deployGpt4 bool = true

param instance string = 'fllm'

@description('Primary location for all resources')
@minLength(1)
param location string

// Optional parameters to override the default azd resource naming conventions.
param resourceGroupName string = ''

var abbrs = loadJsonContent('./abbreviations.json')

// tags that should be applied to all resources.
var tags = {
  'env-name': environmentName
  iac: 'starter/bicep/aca'
  instance: instance
}

// Generate a unique token to be used in naming resources.
// Remove linter suppression after using.
#disable-next-line no-unused-vars
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location, instance))

// Name of the service defined in azure.yaml
#disable-next-line no-unused-vars
var apiServiceName = 'python-api'

// Create the list of OpenAI deployments to add
var deployments = deployGpt4 ? deploymentConfigurations : filter(
  deploymentConfigurations,
  (d) => !contains([ 'gpt-4' ], d.model.name)
)

var deploymentConfigurations = [
  {
    name: 'completions'
    model: {
      format: 'OpenAI'
      name: 'gpt-35-turbo'
      version: '0613'
    }
    sku: {
      capacity: 30
      name: 'Standard'
    }
  }
  {
    name: 'completions4'
    model: {
      format: 'OpenAI'
      name: 'gpt-4'
      version: '1106-Preview'
    }
    sku: {
      capacity: 30
      name: 'Standard'
    }
  }
  {
    name: 'embeddings'
    raiPolicyName: 'Microsoft.Default'
    model: {
      format: 'OpenAI'
      name: 'text-embedding-ada-002'
      version: '2'
    }
    sku: {
      capacity: 30
      name: 'Standard'
    }
  }
]

@description('Timestamp used in naming nested deployments.')
param timestamp string = utcNow()

// Organize resources in a resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: !empty(resourceGroupName) ? resourceGroupName : '${abbrs.resourcesResourceGroups}${location}-${environmentName}'
  location: location
  tags: tags
}

// Modules
module appConfiguration 'core/management/appconfiguration.bicep' = {
  name: 'app-configuration-${timestamp}'
  scope: resourceGroup(rg.name)
  params: {
    location: rg.location
    name: '${abbrs.appConfiguration}-${resourceToken}'
    tags: tags
  }
}

module contentSafety 'core/ai/cognitiveservices.bicep' = {
  name: 'content-safety-${timestamp}'
  scope: resourceGroup(rg.name)
  params: {
    kind: 'ContentSafety'
    location: rg.location
    name: '${abbrs.cognitiveServicesAccounts}content-safety-${resourceToken}'
    tags: tags
  }
}

module cosmosDb 'core/database/cosmos/sql/cosmos-sql-db.bicep' = {
  name: 'cosmos-db-${timestamp}'
  scope: resourceGroup(rg.name)

  params: {
    accountName: '${abbrs.documentDBDatabaseAccounts}nosql-${resourceToken}'
    connectionStringKey: 'foundationallm-cosmosdb-key'
    databaseName: 'database'
    keyVaultName: secureSettings.outputs.name
    location: rg.location
    tags: tags

    containers: [
      {
        id: 'UserSessions'
        maxThroughput: 1000
        name: 'UserSessions'
        partitionKey: '/upn'
      }
      {
        id: 'UserProfiles'
        maxThroughput: 1000
        name: 'UserProfiles'
        partitionKey: '/upn'
      }
      {
        id: 'Sessions'
        maxThroughput: 1000
        name: 'Sessions'
        partitionKey: '/sessionId'
      }
      {
        id: 'leases'
        maxThroughput: 1000
        name: 'leases'
        partitionKey: '/id'
      }
    ]
  }
}

module monitoring 'core/monitor/monitoring.bicep' = {
  name: 'monitoring-${timestamp}'
  scope: resourceGroup(rg.name)
  params: {
    applicationInsightsDashboardName: '${abbrs.portalDashboards}${resourceToken}'
    applicationInsightsName: '${abbrs.insightsComponents}${resourceToken}'
    location: rg.location
    logAnalyticsName: '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    logAnalyticsSku: 'Standalone'
    tags: tags
  }
}

module openai 'core/ai/cognitiveservices.bicep' = {
  name: 'openai-${timestamp}'
  scope: resourceGroup(rg.name)
  params: {
    deployments: deployments
    location: rg.location
    name: '${abbrs.cognitiveServicesAccounts}openai-${resourceToken}'
    tags: tags
  }
}

module searchService 'core/search/search-services.bicep' = {
  name: 'search-service-${timestamp}'
  scope: resourceGroup(rg.name)
  params: {
    authOptions: { apiKeyOnly: {} }
    location: rg.location
    name: '${abbrs.searchSearchServices}index-${resourceToken}'
    sku: { name: 'basic' }
    tags: tags
  }
}

module secureSettings 'core/security/keyvault.bicep' = {
  name: 'secure-settings-${timestamp}'
  scope: resourceGroup(rg.name)
  params: {
    location: rg.location
    name: '${abbrs.keyVaultVaults}sec-set-${resourceToken}'
    tags: tags
  }
}

module storage 'core/storage/storage-account.bicep' = {
  name: 'storage-${timestamp}'
  scope: resourceGroup(rg.name)
  params: {
    location: rg.location
    isHnsEnabled:true
    name: '${abbrs.storageStorageAccounts}dl${resourceToken}'
    tags: tags
  }
}

// Add outputs from the deployment here, if needed.
//
// This allows the outputs to be referenced by other bicep deployments in the deployment pipeline,
// or by the local machine as a way to reference created resources in Azure for local development.
// Secrets should not be added here.
//
// Outputs are automatically saved in the local azd environment .env file.
// To see these outputs, run `azd env get-values`,  or `azd env get-values --output json` for json output.
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
