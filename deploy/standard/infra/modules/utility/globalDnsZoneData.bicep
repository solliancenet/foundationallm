/** Inputs **/
param locations array

/** Locals **/
@description('Private DNS Zones to read.')
var dnsZone = {
  cognitiveservices: 'privatelink.cognitiveservices.azure.com'
  openai: 'privatelink.openai.azure.com'
  search: 'privatelink.search.windows.net'
  sql_server: 'privatelink${environment().suffixes.sqlServerHostname}'
}

var zoneIds = [for (zone, i) in items(privateDnsZone): {
  id: main[i].id
  key: zone.key
  name: main[i].name
}]

var aksDnsZone = [for location in locations: {'aks_${location}': 'privatelink.${location}.azmk8s.io'}]
var crDnsZone = [for location in locations: {'cr_${location}': '${location}.privatelink.azurecr.io'}]

var privateDnsZone = reduce(union(aksDnsZone, crDnsZone), dnsZone, (cur, next, i) => union(cur,next))

/** Outputs **/
@description('Private DNS Zones to use in other modules.')
output ids array = zoneIds

output zones object = privateDnsZone

@description('Private DNS Zones for Storage Accounts')
output idsStorage array = filter(
  zoneIds,
  (zone) => contains([ 'blob', 'dfs', 'file', 'queue', 'table', 'web' ], zone.key)
)

/** Nested Modules **/
@description('Read the specified private DNS zones.')
resource main 'Microsoft.Network/privateDnsZones@2018-09-01' existing = [for zone in items(privateDnsZone): {
  name: zone.value
}]
