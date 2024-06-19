// Inputs
param cidrVnet string = '10.220.128.0/21'
param createVpnGateway bool = false
param environmentName string
param location string
param networkName string=''
param project string
param timestamp string = utcNow()

// Locals
var cidrFllmAuth = cidrSubnet(cidrVnet, 26, 17) // 10.220.132.64/26
var cidrFllmBackend = cidrSubnet(cidrVnet, 24, 1) // 10.220.129.0/24
var cidrFllmFrontend = cidrSubnet(cidrVnet, 24, 2) // 10.220.130.0/24
var cidrFllmOpenAi = cidrSubnet(cidrVnet, 26, 12) // 10.220.131.0/26
var cidrFllmOps = cidrSubnet(cidrVnet, 26, 15) // 10.220.131.192/26
var cidrFllmVec = cidrSubnet(cidrVnet, 26, 16) // 10.220.132.0/26
var cidrNetSvc = cidrSubnet(cidrVnet, 24, 6) // 10.220.134.0/24
var cidrVpnGateway = cidrSubnet(cidrVnet, 24, 5) // 10.220.133.0/24
var name = networkName == '' ? 'vnet-${environmentName}-${location}-net' : networkName
var resourceSuffix = '${environmentName}-${location}-${workload}-${project}'
var workload = 'net'

var subnets = [
  {
    name: 'FLLMBackend'
    addressPrefix: cidrFllmBackend
    inbound: [
      {
        access: 'Allow'
        destinationAddressPrefix: 'VirtualNetwork'
        destinationPortRange: '*'
        name: 'allow-vpn'
        priority: 512
        protocol: '*'
        sourcePortRange: '*'
        sourceAddressPrefixes: [ '172.16.0.0/24' ]
      }
    ]
    serviceEndpoints: [
      {
        service: 'Microsoft.KeyVault'
        locations: [ '*' ]
      }
    ]
  }
  {
    name: 'FLLMFrontEnd'
    addressPrefix: cidrFllmFrontend
    inbound: [
      {
        access: 'Allow'
        destinationAddressPrefix: 'VirtualNetwork'
        destinationPortRange: '*'
        name: 'allow-vpn'
        priority: 512
        protocol: '*'
        sourcePortRange: '*'
        sourceAddressPrefixes: [ '172.16.0.0/24' ]
      }
    ]
    serviceEndpoints: [
      {
        service: 'Microsoft.KeyVault'
        locations: [ '*' ]
      }
    ]
  }
  {
    name: 'GatewaySubnet'
    addressPrefix: cidrVpnGateway
  }
  {
    name: 'FLLMNetSvc'
    addressPrefix: cidrNetSvc
    rules: {
      inbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-vpn'
          priority: 256
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [ '172.16.0.0/24' ]
        }
      ]
    }
    delegations: [
      {
        name: 'Microsoft.Network/dnsResolvers'
        properties: {
          serviceName: 'Microsoft.Network/dnsResolvers'
        }
      }
    ]
  }
  {
    name: 'FLLMOpenAI'
    addressPrefix: cidrFllmOpenAi
    rules: {
      inbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-vpn'
          priority: 512
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [ '172.16.0.0/24' ]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '6390'
          name: 'allow-lb'
          priority: 192
          protocol: 'Tcp'
          sourceAddressPrefix: 'AzureLoadBalancer'
          sourcePortRange: '*'
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-aks-inbound'
          priority: 256
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [
            cidrFllmBackend
          ]
        }
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          name: 'deny-all-inbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
      outbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'Storage'
          destinationPortRange: '443'
          name: 'allow-storage'
          priority: 128
          protocol: 'Tcp'
          sourceAddressPrefix: 'VirtualNetwork'
          sourcePortRange: '*'
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'SQL'
          destinationPortRange: '1443'
          name: 'allow-sql'
          priority: 192
          protocol: 'Tcp'
          sourceAddressPrefix: 'VirtualNetwork'
          sourcePortRange: '*'
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'AzureKeyVault'
          destinationPortRange: '443'
          name: 'allow-kv'
          priority: 224
          protocol: 'Tcp'
          sourceAddressPrefix: 'VirtualNetwork'
          sourcePortRange: '*'
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-vnet'
          priority: 4068
          protocol: '*'
          sourceAddressPrefix: 'VirtualNetwork'
          sourcePortRange: '*'
        }
      ]
    }
    serviceEndpoints: [
      {
        service: 'Microsoft.CognitiveServices' // TODO: Is this needed?
        locations: [ '*' ]
      }
      {
        service: 'Microsoft.Storage'
        locations: [ '*' ]
      }
      {
        service: 'Microsoft.Sql'
        locations: [ '*' ]
      }
      {
        service: 'Microsoft.ServiceBus'
        locations: [ '*' ]
      }
      {
        service: 'Microsoft.KeyVault'
        locations: [ '*' ]
      }
      {
        service: 'Microsoft.EventHub'
        locations: [ '*' ]
      }
    ]
  }
  {
    name: 'FLLMServices'
    addressPrefix: cidrSubnet(cidrVnet, 26, 13)
    rules: {
      inbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-backend-aks'
          priority: 256
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [ cidrFllmBackend ]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-vpn'
          priority: 512
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [ '172.16.0.0/24' ]
        }
        {
          name: 'deny-all-inbound'
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Deny'
          priority: 4096
        }
      ]
    }
    serviceEndpoints: [
      {
        service: 'Microsoft.KeyVault'
        locations: [ '*' ]
      }
    ]
  }
  {
    name: 'FLLMStorage'
    addressPrefix: cidrSubnet(cidrVnet, 26, 14)
    rules: {
      inbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-ops' // TODO: If we end up using a separate subnet for jumpboxes, this will need to change
          priority: 128
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [ cidrFllmOps ]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-vpn'
          priority: 512
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [ '172.16.0.0/24' ]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          direction: 'Inbound'
          name: 'allow-aks-inbound'
          priority: 256
          protocol: '*'
          sourceAddressPrefixes: [ cidrFllmBackend ]
          sourcePortRange: '*'
        }
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          name: 'deny-all-inbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
      outbound: [
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          name: 'deny-all-outbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
    }
    serviceEndpoints: [
      {
        service: 'Microsoft.KeyVault'
        locations: [ '*' ]
      }
    ]
  }
  {
    name: 'ops' // TODO: PLEs.  Maybe put these in FLLMServices?
    addressPrefix: cidrFllmOps
    rules: {
      inbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-ops' // TODO: If we end up using a separate subnet for jumpboxes, this will need to change
          priority: 128
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [ cidrFllmOps ]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-vpn'
          priority: 512
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [ '172.16.0.0/24' ]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-aks-inbound'
          priority: 264
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [
            cidrFllmFrontend
            cidrFllmBackend
          ]
        }
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          name: 'deny-all-inbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
    }
    serviceEndpoints: [
      {
        service: 'Microsoft.KeyVault'
        locations: [ '*' ]
      }
    ]
  }
  {
    name: 'Vectorization'
    addressPrefix: cidrFllmVec
    rules: {
      inbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-vpn'
          priority: 512
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [ '172.16.0.0/24' ]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-aks-inbound'
          priority: 256
          protocol: '*'
          sourceAddressPrefixes: [ cidrFllmBackend ]
          sourcePortRange: '*'
        }
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          name: 'deny-all-inbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
      outbound: [
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          direction: 'Outbound'
          name: 'deny-all-outbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
    }
    serviceEndpoints: [
      {
        service: 'Microsoft.KeyVault'
        locations: [ '*' ]
      }
    ]
  }
  {
    name: 'FLLMAuth'
    addressPrefix: cidrFllmAuth
    rules: {
      inbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-ops' // TODO: If we end up using a separate subnet for jumpboxes, this will need to change
          priority: 128
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [ cidrFllmOps ]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-vpn'
          priority: 512
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [ '172.16.0.0/24' ]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          direction: 'Inbound'
          name: 'allow-aks-inbound'
          priority: 256
          protocol: '*'
          sourceAddressPrefixes: [ cidrFllmBackend ]
          sourcePortRange: '*'
        }
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          name: 'deny-all-inbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
      outbound: [
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          name: 'deny-all-outbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
    }
    serviceEndpoints: [
      {
        service: 'Microsoft.KeyVault'
        locations: [ '*' ]
      }
    ]
  }
]

var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'Networking'
}

// Outputs
output vnetId string = main.id
output vnetName string = main.name

// Resources
resource main 'Microsoft.Network/virtualNetworks@2023-05-01' = {
  name: name
  location: location
  tags: tags

  properties: {
    enableDdosProtection: false
    addressSpace: {
      addressPrefixes: [ cidrVnet ]
    }
    subnets: [for (subnet, i) in subnets: {
      name: subnet.name
      properties: {
        addressPrefix: subnet.addressPrefix
        privateEndpointNetworkPolicies: 'Enabled'
        privateLinkServiceNetworkPolicies: 'Enabled'
        serviceEndpoints: subnet.?serviceEndpoints
        delegations: subnet.?delegations

        networkSecurityGroup: subnet.name == 'GatewaySubnet' ? null : {
          id: nsg[i].outputs.id
        }
      }
    }]
  }
}

// Nested Modules
module nsg 'modules/nsg.bicep' = [for subnet in subnets: if (subnet.name != 'GatewaySubnet') {
  name: 'nsg-${subnet.name}-${timestamp}'
  params: {
    location: location
    resourceSuffix: '${name}-${subnet.name}'
    rules: subnet.?rules
    tags: tags
  }
}]

module vpn 'modules/vpnGateway.bicep' = if (createVpnGateway) {
  name: 'vpnGw-${timestamp}'
  params: {
    location: location
    resourceSuffix: resourceSuffix
    subnetId: '${main.id}/subnets/GatewaySubnet'
  }
}
