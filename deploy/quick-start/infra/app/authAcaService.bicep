param name string
param location string = resourceGroup().location
param tags object = {}
param authRgName string
param authStoreName string
param cpu string
param memory string
param replicaCount int
param identityName string
param keyvaultName string
param containerAppsEnvironmentName string
param applicationInsightsName string
param exists bool
@secure()
param appDefinition object
param hasIngress bool = true
param envSettings array = []
param secretSettings array = []
param serviceName string
param imageName string

var formattedAppName = replace(name, '-', '')
var truncatedAppName = substring(formattedAppName, 0, min(length(formattedAppName), 32))

var appSettingsArray = filter(array(appDefinition.settings), i => i.name != '')
var secrets = union(map(filter(appSettingsArray, i => i.?secret != null), i => {
  name: i.name
  value: i.value
  secretRef: i.?secretRef ?? take(replace(replace(toLower(i.name), '_', '-'), '.', '-'), 32)
}), secretSettings)

var env = union(map(filter(appSettingsArray, i => i.?secret == null), i => {
  name: i.name
  value: i.value
}), envSettings)

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-04-01-preview' existing = {
  name: containerAppsEnvironmentName
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

module authRoles '../shared/auth-roles.bicep' = {
  name: 'auth-roles'
  scope: resourceGroup(authRgName)
  params: {
    authStoreName: authStoreName
    keyvaultName: keyvaultName
    identityId: identity.id
    principalId: identity.properties.principalId
  }
}

module fetchLatestImage '../modules/fetch-container-image.bicep' = {
  name: '${name}-fetch-image'
  params: {
    exists: exists
    name: imageName
  }
}

resource app 'Microsoft.App/containerApps@2023-04-01-preview' = {
  name: truncatedAppName
  location: location
  tags: union(tags, {'azd-service-name':  serviceName })
  dependsOn: [ authRoles ]
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: { '${identity.id}': {} }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: hasIngress ? {
        external: true
        targetPort: 80
        transport: 'auto'
      } : null
      secrets: union([
      ],
      map(secrets, secret => {
        identity: identity.id
        keyVaultUrl: secret.value
        name: secret.secretRef
      }))
    }
    template: {
      containers: [
        {
          image: fetchLatestImage.outputs.?containers[?0].?image ?? imageName
          name: 'main'
          env: union([
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
          map(secrets, secret => {
            name: secret.name
            secretRef: secret.secretRef
          }))
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

output defaultDomain string = containerAppsEnvironment.properties.defaultDomain
output name string = app.name
output uri string = hasIngress ? 'https://${app.properties.configuration.ingress.fqdn}' : ''
output id string = app.id
