/** Inputs **/
@description('Location for all resources')
param location string

@description('Target Kubernetes namespace for microservice deployment')
param namespace string

@description('OIDC Issuer URL')
param oidcIssuerUrl string

@description('Resource suffix for all resources')
param resourceSuffix string

@description('Service name')
param serviceName string

@description('Tags for all resources')
param tags object

@description('Timestamp for nested deployments')
param timestamp string = utcNow()

/** Outputs **/
@description('Service Managed Identity Client Id.')
output serviceClientId string = managedIdentity.properties.clientId

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

@description('Auth Role assignments for microservice managed identity')
module authRoleAssignments 'utility/roleAssignments.bicep' = {
  name: 'authIAM-${serviceName}-${timestamp}'
  scope: resourceGroup()
  params: {
    principalId: managedIdentity.properties.principalId
    roleDefinitionNames: [
      'Contributor'
      'Key Vault Secrets Officer'
      'Storage Blob Data Contributor'
    ]
  }
}
