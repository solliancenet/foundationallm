/** Inputs **/
param apiKeySecretName string
param applicationInsightsName string
param containerAppsEnvironmentName string
param cpu string
param envSettings array = []
param exists bool
param hasIngress bool = false
param identityName string
param imageName string
param keyvaultName string
param location string = resourceGroup().location
param memory string
param name string
param replicaCount int
param resourceToken string
param secretSettings array = []
param serviceName string
param tags object = {}
param timestamp int = dateTimeToEpoch(utcNow())

@secure()
param appDefinition object

/** Locals **/
var appSettingsArray = filter(array(appDefinition.settings), i => i.name != '')
var formattedAppName = replace('${name}${resourceToken}', '-', '')
var truncatedAppName = substring(formattedAppName, 0, min(length(formattedAppName), 32))

var env = union(
  map(
    filter(appSettingsArray, i => i.?secret == null),
    i => {
      name: i.name
      value: i.value
    }
  ),
  envSettings
)

var secretNames = [
  apiKeySecretName
]

var secrets = union(
  map(
    filter(appSettingsArray, i => i.?secret != null),
    i => {
      name: i.name
      value: i.value
      secretRef: i.?secretRef ?? take(replace(replace(toLower(i.name), '_', '-'), '.', '-'), 32)
    }
  ),
  secretSettings
)

var serviceRoleAssignments = concat(commonRoleAssignments, keyVaultRoleAssignments, certificateRoleAssignments)
var commonRoleAssignments = [
  'App Configuration Data Reader'
  'Storage Blob Data Contributor'
  'Storage Queue Data Contributor'
  'EventGrid Contributor'
]

var keyVaultRoleAssignments = contains(['management-api'], serviceName)
  ? ['Key Vault Secrets Officer']
  : ['Key Vault Secrets User']

var certificateRoleAssignments = contains(['vectorization-api','vectorization-job'], serviceName)
  ? ['Key Vault Certificate User']
  : []

/** Data Sources **/
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-04-01-preview' existing = {
  name: containerAppsEnvironmentName
}

resource keyvault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyvaultName
}

/** Resources **/
resource app 'Microsoft.App/containerApps@2023-04-01-preview' = {
  name: truncatedAppName
  location: location
  tags: union(tags, { 'azd-service-name': serviceName })
  dependsOn: [secretsAccessPolicy, roleAssignments]
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: { '${identity.id}': {} }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: hasIngress
        ? {
            external: true
            targetPort: 80
            transport: 'auto'
          }
        : null
      secrets: union(
        [],
        map(
          secrets,
          secret => {
            identity: identity.id
            keyVaultUrl: secret.value
            name: secret.secretRef
          }
        )
      )
    }
    template: {
      containers: [
        {
          image: fetchLatestImage.outputs.?containers[?0].?image ?? imageName
          name: 'main'
          env: union(
            [
              {
                name: 'AZURE_CLIENT_ID'
                value: identity.properties.clientId
              }
              {
                name: 'AZURE_TENANT_ID'
                value: identity.properties.tenantId
              }
              {
                name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
                value: applicationInsights.properties.ConnectionString
              }
              {
                name: 'PORT'
                value: '80'
              }
            ],
            env,
            map(
              secrets,
              secret => {
                name: secret.name
                secretRef: secret.secretRef
              }
            )
          )
          resources: {
            cpu: json(cpu)
            memory: memory
          }
        }
      ]
      scale: replicaCount > 0 ? {
        minReplicas: replicaCount
        maxReplicas: replicaCount
      } : {
        minReplicas: 1
        maxReplicas: 10
      }
    }
    workloadProfileName: 'Warm'
  }
}

resource apiKey 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = [
  for secretName in secretNames: {
    name: secretName
    parent: keyvault
    tags: tags
    properties: {
      value: uniqueString(subscription().id, resourceGroup().id, app.id, serviceName)
    }
  }
]

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

//TODO: do we need this?  Key Vault is configured for RBAC
resource secretsAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: keyvault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        objectId: identity.properties.principalId
        permissions: { secrets: ['get', 'list'] }
        tenantId: subscription().tenantId
      }
    ]
  }
}

/** Nested Modules **/
module fetchLatestImage '../modules/fetch-container-image.bicep' = {
  name: '${name}-fetch-image-${timestamp}'
  params: {
    exists: exists
    name: imageName
  }
}

module roleAssignments '../shared/roleAssignments.bicep' = {
  scope: resourceGroup()
  name: '${name}-roles-${timestamp}'
  params: {
    principalId: identity.properties.principalId
    roleDefinitionNames: serviceRoleAssignments
  }
}

output defaultDomain string = containerAppsEnvironment.properties.defaultDomain
output id string = app.id
output miPrincipalId string = identity.properties.principalId
output name string = app.name
output uri string = hasIngress ? 'https://${app.properties.configuration.ingress.fqdn}' : ''
