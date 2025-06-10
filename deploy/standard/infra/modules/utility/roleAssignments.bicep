// See: https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
param principalId string
param roleDefinitionIds object = {}
param principalType string = 'ServicePrincipal'
param roleDefinitionNames array = []

/** Locals **/
var roleDefinitionsToCreate = union(selectedRoleDefinitions, items(roleDefinitionIds))
var selectedRoleDefinitions = filter(items(roleDefinitions), (item) => contains(roleDefinitionNames, item.key))
var roleDefinitions = {
  'App Configuration Data Reader':         '516239f1-63e1-4d78-a4de-a74fb236a071'
  'Cognitive Services OpenAI Contributor': 'a001fd3d-188f-4b5d-821b-7da978bf7442'
  'Cognitive Services OpenAI User':        '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
  'Cognitive Services User':               'a97b65f3-24c7-4388-baec-2e87135dc908'
  'Contributor':                           'b24988ac-6180-42a0-ab88-20f7382dd24c'
  'EventGrid Contributor':                 '1e241071-0855-49ea-94dc-649edcd759de'
  'Key Vault Certificate User':            'db79e9a7-68ee-4b58-9aeb-b90e7c24fcba'
  'Key Vault Certificates Officer':        'a4417e6f-fecd-4de8-b567-7b0420556985'
  'Key Vault Secrets Officer':             'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
  'Key Vault Secrets User':                '4633458b-17de-408a-b874-0445c86b69e6'
  'Reader':                                'acdd72a7-3385-48ef-bd42-f606fba81ae7'
  'Storage Blob Data Contributor':         'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
  'Storage Queue Data Contributor':        '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
  'Azure ContainerApps Session Executor':  '0fb8eba5-a2bb-4abe-b1c1-49dfad359bb0'
}

var roleAssignmentsToCreate = [
  for roleDefinitionId in roleDefinitionsToCreate: {
    name: guid(principalId, resourceGroup().id, roleDefinitionId.value)
    roleDefinitionId: roleDefinitionId.value
  }
]

/** Resources **/
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = [
  for roleAssignmentToCreate in roleAssignmentsToCreate: {
    name: roleAssignmentToCreate.name
    properties: {
      principalType: principalType
      principalId: principalId
      roleDefinitionId: subscriptionResourceId(
        'Microsoft.Authorization/roleDefinitions',
        roleAssignmentToCreate.roleDefinitionId
      )
    }
  }
]
