/** Inputs **/
@description('API Key for service')
@secure()
param apiKey string = newGuid()

@description('Location for all resources')
param location string

@description('Target Kubernetes namespace for microservice deployment')
param namespace string

@description('OIDC Issuer URL')
param oidcIssuerUrl string

@description('OPS Resource Group name')
param opsResourceGroupName string

@description('OPS resource suffix')
param opsResourceSuffix string

@description('Resource suffix for all resources')
param resourceSuffix string

param secretName string

@description('Service name')
param serviceName string

@description('Storage Resource Group name')
param storageResourceGroupName string

@description('Tags for all resources')
param tags object

@description('Timestamp for nested deployments')
param timestamp string = utcNow()

@description('Flag enabling OIDC support.')
param useOidc bool = false

/** Locals **/
@description('Formatted untruncated resource name')
var kvFormattedName = toLower('${kvServiceType}-${substring(opsResourceSuffix, 0, length(opsResourceSuffix) - 4)}')

@description('The Resource Name')
var kvTruncatedName = substring(kvFormattedName, 0, min([length(kvFormattedName), 20]))
var kvName = '${kvTruncatedName}-${substring(opsResourceSuffix, length(opsResourceSuffix) - 3, 3)}'

@description('The Resource Service Type token')
var kvServiceType = 'kv'

var opsRoleAssignmentIds = union(commonOpsRoleAssignments, keyVaultOpsRoleAssignments, certificateOpsRoleAssignmentIds)
var commonOpsRoleAssignments = {
  'App Configuration Data Reader': '516239f1-63e1-4d78-a4de-a74fb236a071'
}

var keyVaultOpsRoleAssignments = contains(['management-api'], serviceName)
  ? {
      'Key Vault Secrets Officer': 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
    }
  : {
      'Key Vault Secrets User': '4633458b-17de-408a-b874-0445c86b69e6'
    }

var certificateOpsRoleAssignmentIds = contains(['vectorization-api', 'vectorization-job'], serviceName)
  ? {
      'Key Vault Certificate User': 'db79e9a7-68ee-4b58-9aeb-b90e7c24fcba'
    }
  : {}

/** Outputs **/
@description('Service Managed Identity Client Id.')
output serviceClientId string = managedIdentity.properties.clientId
output servicePrincipalId string = managedIdentity.properties.principalId
output serviceMiName string = managedIdentity.name

@description('Service Api Key Secret KeyVault Uri.')
#disable-next-line outputs-should-not-contain-secrets
output serviceApiKeySecretUri string = useOidc ? '' : apiKeySecret.outputs.secretUri

/** Resources **/
@description('Resource for configuring user managed identity for a microservice')
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  location: location
  name: 'mi-${serviceName}-${resourceSuffix}'
  tags: tags
}

@description('OIDC Federated Identity Credential for managed identity for a microservice')
resource federatedIdentityCredential 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  name: serviceName
  parent: managedIdentity
  properties: {
    audiences: ['api://AzureADTokenExchange']
    issuer: oidcIssuerUrl
    subject: 'system:serviceaccount:${namespace}:${serviceName}'
  }
}

@description('OPS Role assignments for microservice managed identity')
module opsRoleAssignments 'utility/roleAssignments.bicep' = {
  name: 'opsIAM-${serviceName}-${timestamp}'
  scope: resourceGroup(opsResourceGroupName)
  params: {
    principalId: managedIdentity.properties.principalId
    roleDefinitionIds: opsRoleAssignmentIds
  }
}

@description('APP Role assignments for microservice managed identity')
module appRoleAssignments 'utility/roleAssignments.bicep' = {
  name: 'appIAM-${serviceName}-${timestamp}'
  scope: resourceGroup()
  params: {
    principalId: managedIdentity.properties.principalId
    roleDefinitionIds: {
      'EventGrid Contributor': '1e241071-0855-49ea-94dc-649edcd759de'
    }
  }
}

@description('Storage Role assignments for microservice managed identity')
module storageRoleAssignments 'utility/roleAssignments.bicep' = {
  name: 'storageIAM-${serviceName}-${timestamp}'
  scope: resourceGroup(storageResourceGroupName)
  params: {
    principalId: managedIdentity.properties.principalId
    roleDefinitionIds: {
      Contributor: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
      'Storage Blob Data Contributor': 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
      'Storage Queue Data Contributor': '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
    }
  }
}

@description('API Key for microservice (only created if not using Entra)')
module apiKeySecret 'kvSecret.bicep' = if (!useOidc) {
  name: 'apiKey-${serviceName}-${timestamp}'
  scope: resourceGroup(opsResourceGroupName)
  params: {
    kvName: kvName
    secretName: secretName
    secretValue: apiKey
    tags: tags
  }
}
