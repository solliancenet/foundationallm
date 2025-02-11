// Inputs
param cidrVnet string = '10.220.128.0/20'
param environmentName string
param hubResourceGroup string
param hubSubscriptionId string = subscription().subscriptionId
param hubVnetName string

param globalDnsResourceGroup string
param regionalDnsResourceGroup string
param dnsSubscriptionId string = subscription().subscriptionId

param location string
param networkName string = ''
param project string
param timestamp string = utcNow()
param allowedExternalCidr string

// Locals
@description('Private DNS Zones to link.')
var globalPrivateDnsZone = {
  cognitiveservices: 'privatelink.cognitiveservices.azure.com'
  openai: 'privatelink.openai.azure.com'
  search: 'privatelink.search.windows.net'
  sql_server: 'privatelink${environment().suffixes.sqlServerHostname}'
  aks: 'privatelink.${location}.azmk8s.io'
  cr_region: '${location}.privatelink.azurecr.io'
}

var privateDnsZone = {
  //  agentsvc: 'privatelink.agentsvc.azure-automation.net'
  blob: 'privatelink.blob.${environment().suffixes.storage}'
  configuration_stores: 'privatelink.azconfig.io'
  cosmosdb: 'privatelink.documents.azure.com'
  cr: 'privatelink.azurecr.io'
  dfs: 'privatelink.dfs.${environment().suffixes.storage}'
  eventgrid: 'privatelink.eventgrid.azure.net'
  file: 'privatelink.file.${environment().suffixes.storage}'
  // monitor: 'privatelink.monitor.azure.com'
  // ods: 'privatelink.ods.opinsights.azure.com'
  // oms: 'privatelink.oms.opinsights.azure.com'
  queue: 'privatelink.queue.${environment().suffixes.storage}'
  // sites: 'privatelink.azurewebsites.net'
  table: 'privatelink.table.${environment().suffixes.storage}'
  vault: 'privatelink.vaultcore.azure.net'
}

var opsSubnetCidr = cidrSubnet(cidrVnet, 26, 0) // 10.220.128.0/26
var servicesSubnetCidr = cidrSubnet(cidrVnet, 26, 1) // 10.220.128.64/26
var authSubnetCidr = cidrSubnet(cidrVnet, 26, 2) // 10.220.128.128/26
var openAiSubnetCidr = cidrSubnet(cidrVnet, 26, 3) // 10.220.128.192/26
var storageSubnetCidr = cidrSubnet(cidrVnet, 26, 4) // 10.220.129.0/26
var vectorizationSubnetCidr = cidrSubnet(cidrVnet, 26, 5) // 10.220.129.64/26
var backendAksSubnetCidr = cidrSubnet(cidrVnet, 22, 1) // 10.220.132.0/22
var frontendAksSubnetCidr = cidrSubnet(cidrVnet, 22, 2) // 10.220.140.0/22
// TODO: Use Namer FUnction from main.bicep
var name = networkName == '' ? 'vnet-${environmentName}-${location}-net' : networkName

var subnets = [
  {
    name: 'aks-backend'
    addressPrefix: backendAksSubnetCidr
    privateLinkServiceNetworkPolicies: 'Disabled'
    inbound: [
      {
        access: 'Allow'
        destinationAddressPrefix: 'VirtualNetwork'
        destinationPortRange: '*'
        name: 'allow-vpn'
        priority: 512
        protocol: '*'
        sourcePortRange: '*'
        sourceAddressPrefixes: [allowedExternalCidr]
      }
    ]
    serviceEndpoints: [
      {
        service: 'Microsoft.KeyVault'
        locations: ['*']
      }
    ]
  }
  {
    name: 'aks-frontend'
    addressPrefix: frontendAksSubnetCidr
    privateLinkServiceNetworkPolicies: 'Disabled'
    inbound: [
      {
        access: 'Allow'
        destinationAddressPrefix: 'VirtualNetwork'
        destinationPortRange: '*'
        name: 'allow-vpn'
        priority: 512
        protocol: '*'
        sourcePortRange: '*'
        sourceAddressPrefixes: [allowedExternalCidr]
      }
    ]
    serviceEndpoints: [
      {
        service: 'Microsoft.KeyVault'
        locations: ['*']
      }
    ]
  }
  {
    name: 'openai'
    addressPrefix: openAiSubnetCidr
    privateLinkServiceNetworkPolicies: 'Enabled'
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
          sourceAddressPrefixes: [allowedExternalCidr]
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
            backendAksSubnetCidr
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
        locations: ['*']
      }
      {
        service: 'Microsoft.Storage'
        locations: ['*']
      }
      {
        service: 'Microsoft.Sql'
        locations: ['*']
      }
      {
        service: 'Microsoft.ServiceBus'
        locations: ['*']
      }
      {
        service: 'Microsoft.KeyVault'
        locations: ['*']
      }
      {
        service: 'Microsoft.EventHub'
        locations: ['*']
      }
    ]
  }
  {
    name: 'services'
    addressPrefix: servicesSubnetCidr
    privateLinkServiceNetworkPolicies: 'Enabled'
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
          sourceAddressPrefixes: [backendAksSubnetCidr]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-vpn'
          priority: 512
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [allowedExternalCidr]
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
        locations: ['*']
      }
    ]
  }
  {
    name: 'storage'
    addressPrefix: storageSubnetCidr
    privateLinkServiceNetworkPolicies: 'Enabled'
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
          sourceAddressPrefixes: [opsSubnetCidr]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-vpn'
          priority: 512
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [allowedExternalCidr]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          direction: 'Inbound'
          name: 'allow-aks-inbound'
          priority: 256
          protocol: '*'
          sourceAddressPrefixes: [backendAksSubnetCidr]
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
        locations: ['*']
      }
    ]
  }
  {
    name: 'ops' // TODO: PLEs.  Maybe put these in services?
    addressPrefix: opsSubnetCidr
    privateLinkServiceNetworkPolicies: 'Enabled'
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
          sourceAddressPrefixes: [opsSubnetCidr]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-vpn'
          priority: 512
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [allowedExternalCidr]
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
            frontendAksSubnetCidr
            backendAksSubnetCidr
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
        locations: ['*']
      }
    ]
  }
  {
    name: 'vectorization'
    addressPrefix: vectorizationSubnetCidr
    privateLinkServiceNetworkPolicies: 'Enabled'
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
          sourceAddressPrefixes: [allowedExternalCidr]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-aks-inbound'
          priority: 256
          protocol: '*'
          sourceAddressPrefixes: [backendAksSubnetCidr]
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
        locations: ['*']
      }
    ]
  }
  {
    name: 'auth'
    addressPrefix: authSubnetCidr
    privateLinkServiceNetworkPolicies: 'Enabled'
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
          sourceAddressPrefixes: [opsSubnetCidr]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-vpn'
          priority: 512
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [allowedExternalCidr]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          direction: 'Inbound'
          name: 'allow-aks-inbound'
          priority: 256
          protocol: '*'
          sourceAddressPrefixes: [backendAksSubnetCidr]
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
        locations: ['*']
      }
    ]
  }
]

var tags = {
  'azd-env-name': environmentName
  'iac-type': 'bicep'
  'project-name': project
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
      addressPrefixes: [cidrVnet]
    }
    subnets: [
      for (subnet, i) in subnets: {
        name: subnet.name
        properties: {
          addressPrefix: subnet.addressPrefix
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Disabled'
          serviceEndpoints: subnet.?serviceEndpoints
          delegations: subnet.?delegations

          networkSecurityGroup: {
            id: nsg[i].outputs.id
          }
        }
      }
    ]
  }
}

// Nested Modules
module nsg 'modules/nsg.bicep' = [
  for subnet in subnets: if (subnet.name != 'GatewaySubnet') {
    name: 'nsg-${subnet.name}-${timestamp}'
    params: {
      location: location
      resourceSuffix: '${name}-${subnet.name}'
      rules: subnet.?rules
      tags: tags
    }
  }
]

@description('Use the preexisting specified private DNS zones.')
module dns './modules/dns.bicep' = [for zone in items(privateDnsZone): {
  name: '${zone.value}-${timestamp}'
  scope: resourceGroup(dnsSubscriptionId, regionalDnsResourceGroup)
  params: {
    key: zone.key
    vnetId: main.id
    vnetName: main.name
    zone: zone.value

    tags: {
      Environment: environmentName
      IaC: 'Bicep'
      Project: project
      Purpose: 'Networking'
    }
  }
}]

@description('Use the preexisting specified private DNS zones.')
module globalDns './modules/dns.bicep' = [for zone in items(globalPrivateDnsZone): {
  name: '${zone.value}-${timestamp}'
  scope: resourceGroup(dnsSubscriptionId, globalDnsResourceGroup)
  params: {
    key: zone.key
    vnetId: main.id
    vnetName: main.name
    zone: zone.value

    tags: {
      Environment: environmentName
      IaC: 'Bicep'
      Project: project
      Purpose: 'Networking'
    }
  }
}]

resource hubVnet 'Microsoft.Network/virtualNetworks@2024-01-01' existing = {
  name: hubVnetName
  scope: resourceGroup(hubSubscriptionId, hubResourceGroup)
}

output hubVnetId string = hubVnet.id
