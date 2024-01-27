metadata description = 'Creates an Azure Virtual Network.'
param addressPrefix string
param location string = resourceGroup().location
param name string
param subnets array = []
param tags object = {}

resource network 'Microsoft.Network/virtualNetworks@2023-05-01' = {
  location: location
  name: name
  tags: tags

  properties: {
    addressSpace: {      addressPrefixes: [ addressPrefix ]    }
    enableDdosProtection: false
    subnets: [for (subnet, i) in subnets: {
      name: subnet.name
      properties: {
        addressPrefix: subnet.addressPrefix
        privateEndpointNetworkPolicies: 'Disabled'
        privateLinkServiceNetworkPolicies: 'Enabled'
        serviceEndpoints: subnet.?serviceEndpoints
        delegations: subnet.?delegations // TODO loop over it
        networkSecurityGroup: subnet.?networkSecurityGroup // TODO loop over it
      }
    }]
  }
}
