param displayName string = name
param hubName string
param keyVaultName string
param location string = resourceGroup().location
param name string
param resourceToken string
param skuName string = 'Basic'
param tags object = {}
param timestamp string = utcNow()

@allowed(['Basic', 'Free', 'Premium', 'Standard'])
param skuTier string = 'Basic'

@allowed(['Enabled', 'Disabled'])
param publicNetworkAccess string = 'Enabled'

// Data Sources
resource hub 'Microsoft.MachineLearningServices/workspaces@2024-01-01-preview' existing = {
  name: hubName
}

// Resources
resource project 'Microsoft.MachineLearningServices/workspaces@2024-01-01-preview' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: skuName
    tier: skuTier
  }
  kind: 'Project'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    friendlyName: displayName
    hbiWorkspace: false
    v1LegacyMode: false
    publicNetworkAccess: publicNetworkAccess
    hubResourceId: hub.id
  }
}

resource onlineEndpointPhi 'Microsoft.MachineLearningServices/workspaces/onlineEndpoints@2024-04-01' = {
  kind: 'Managed'
  location: location
  name: 'oe-phi-${resourceToken}'
  parent: project
  tags: tags

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    authMode: 'Key'
    publicNetworkAccess: publicNetworkAccess

    properties: {
      SharedComputCapacityEnabled: 'true'
    }
  }
}

resource onlineEndpointMistral 'Microsoft.MachineLearningServices/workspaces/onlineEndpoints@2024-04-01' = {
  kind: 'Managed'
  location: location
  name: 'oe-mistral-${resourceToken}'
  parent: project
  tags: tags

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    authMode: 'Key'
    publicNetworkAccess: publicNetworkAccess

    properties: {
      SharedComputCapacityEnabled: 'true'
    }
  }
}

resource deploymentPhiMini 'Microsoft.MachineLearningServices/workspaces/onlineEndpoints/deployments@2024-04-01' = {
  kind: 'Managed'
  location: resourceGroup().location
  name: 'phi-3-mini-128k-instruct-7'
  parent: onlineEndpointPhi
  tags: tags
  properties: {
    egressPublicNetworkAccess: publicNetworkAccess
    endpointComputeType: 'Managed'
    instanceType: 'Standard_NC24ads_A100_v4'
    model: 'azureml://registries/azureml/models/Phi-3-mini-128k-instruct/versions/7'
    environmentVariables: {
      AZUREML_MODEL_DIR: '/var/azureml-app/azureml-models/Phi-3-mini-128k-instruct/7'
    }
    scaleSettings: {
      scaleType: 'Default'
    }
    requestSettings: {
      maxConcurrentRequestsPerInstance: 1
      maxQueueWait: 'PT0S'
      requestTimeout: 'PT1M30S'
    }
    livenessProbe: {
      failureThreshold: 30
      initialDelay: 'PT10M'
      period: 'PT10S'
      successThreshold: 1
      timeout: 'PT2S'
    }
    readinessProbe: {
      failureThreshold: 30
      initialDelay: 'PT10S'
      period: 'PT10S'
      successThreshold: 1
      timeout: 'PT2S'
    }
  }
  sku: {
    capacity: 1
    name: skuName
    tier: skuTier
  }
}

resource deploymentMixtral 'Microsoft.MachineLearningServices/workspaces/onlineEndpoints/deployments@2024-04-01' = {
  location: resourceGroup().location
  kind: 'Managed'
  name: 'mistralai-mistral-7b-instruc-10'
  parent: onlineEndpointMistral
  tags: tags
  properties: {
    appInsightsEnabled: false
    egressPublicNetworkAccess: publicNetworkAccess
    endpointComputeType: 'Managed'
    instanceType: 'Standard_NC24ads_A100_v4'
    model: 'azureml://registries/azureml/models/mistralai-Mistral-7B-Instruct-v01/versions/10'
    environmentVariables: {
      AZUREML_MODEL_DIR: '/var/azureml-app/azureml-models/mistralai-Mistral-7B-Instruct-v01/10'
    }
    livenessProbe: {
      failureThreshold: 60
      initialDelay: 'PT15M'
      period: 'PT10S'
      successThreshold: 1
      timeout: 'PT2S'
    }
    readinessProbe: {
      failureThreshold: 30
      initialDelay: 'PT10S'
      period: 'PT10S'
      successThreshold: 1
      timeout: 'PT2S'
    }
    requestSettings: {
      maxConcurrentRequestsPerInstance: 1
      maxQueueWait: 'PT0S'
      requestTimeout: 'PT1M30S'
    }
    scaleSettings: {
      scaleType: 'Default'
    }
  }

  sku: {
    capacity: 1
    name: skuName
    tier: skuTier
  }
}

// Nested Modules
module keyVaultAccess '../security/keyvault-access.bicep' = {
  name: 'keyvault-access-${timestamp}'
  params: {
    keyVaultName: keyVaultName
    principalId: project.identity.principalId
  }
}

module mlServiceRoleDataScientist '../security/role.bicep' = {
  name: 'ml-service-role-data-scientist-${timestamp}'
  params: {
    principalId: project.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: 'f6c7c914-8db3-469d-8ca1-694a8f32e121'
  }
}

module mlServiceRoleSecretsReader '../security/role.bicep' = {
  name: 'ml-service-role-secrets-reader-${timestamp}'
  params: {
    principalId: project.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: 'ea01e6af-a1c1-4350-9563-ad00f8c72ec5'
  }
}

// Outputs
output id string = project.id
output name string = project.name
output principalId string = project.identity.principalId
output mistralDeploymentName string = deploymentMixtral.name
output mistralEndpointName string = onlineEndpointMistral.name
output phiDeploymentName string = deploymentPhiMini.name
output phiEndpointName string = onlineEndpointPhi.name
