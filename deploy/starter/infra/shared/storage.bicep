param containers array = []
param files array = []
param queues array = []
param keyvaultName string
param location string = resourceGroup().location
param name string
param tags object = {}

var secretNames = [
  'storage-connection'
  'foundationallm-agent-resourceprovider-storage-connectionstring'
  'foundationallm-agenthub-storagemanager-blobstorage-connectionstring'
  'foundationallm-blobstoragememorysource-blobstorageconnection'
  'foundationallm-cognitivesearchmemorysource-blobstorageconnection'
  'foundationallm-configuration-resourceprovider-storage-connectionstring'
  'foundationallm-datasourcehub-storagemanager-blobstorage-connectionstring'
  'foundationallm-durablesystemprompt-blobstorageconnection'
  'foundationallm-prompt-resourceprovider-storage-connectionstring'
  'foundationallm-prompthub-storagemanager-blobstorage-connectionstring'
  'foundationallm-vectorization-queues-connectionstring'
  'foundationallm-vectorization-state-connectionstring'
  'foundationallm-vectorization-resourceprovider-storage-connectionstring'
]

resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: name
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    isHnsEnabled: true
  }
  tags: tags
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storage
  name: 'default'
}

resource blobContainers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = [
  for container in containers: {
    parent: blobService
    name: container.name
  }
]

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2023-01-01' = {
  parent: storage
  name: 'default'
}

resource storageQueues 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = [
  for queue in queues: {
    parent: queueService
    name: queue.name
  }
]

resource blobFiles 'Microsoft.Resources/deploymentScripts@2020-10-01' = [
  for file in files: {
    name: file.file
    location: location
    kind: 'AzureCLI'
    properties: {
      azCliVersion: '2.26.1'
      timeout: 'PT5M'
      retentionInterval: 'PT1H'
      environmentVariables: [
        {
          name: 'AZURE_STORAGE_ACCOUNT'
          value: storage.name
        }
        {
          name: 'AZURE_STORAGE_KEY'
          secureValue: storage.listKeys().keys[0].value
        }
      ]
      scriptContent: 'echo "${file.content}" > ${file.file} && az storage blob upload -f ${file.file} -c ${file.container} -n ${file.path}'
    }
    dependsOn: [ blobContainers ]
  }
]

resource keyvault 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
  name: keyvaultName
}

resource storageConnectionString 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = [
  for secretName in secretNames: {
    name: secretName
    parent: keyvault
    tags: tags
    properties: {
      value: 'DefaultEndpointsProtocol=https;AccountName=${name};AccountKey=${storage.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
    }
  }
]

output connectionSecretName string = storageConnectionString[0].name
output connectionSecretRef string = storageConnectionString[0].properties.secretUri
output name string = storage.name
