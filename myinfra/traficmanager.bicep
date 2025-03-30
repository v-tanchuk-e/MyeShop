param trafficMngrProfileName string
param webMainId string
param webFailoverId string
param epoch string = '${dateTimeToEpoch(dateTimeAdd(utcNow(), 'P1Y'))}'
var suffix = uniqueString(subscription().id, epoch)

resource trafficMng 'Microsoft.Network/trafficManagerProfiles@2018-08-01' = {
  name: trafficMngrProfileName
  location: 'global'
  properties: {
    profileStatus: 'Enabled'
    trafficRoutingMethod: 'Priority'
    dnsConfig: {
      relativeName: trafficMngrProfileName
      ttl: 60
    }
    monitorConfig: {
      profileMonitorStatus: 'Enabled'
      protocol: 'HTTPS'
      port: 443
      path: '/health'
    }
    endpoints: [
      {
        name: 'ne-web-${suffix}'
        type: 'Microsoft.Network/trafficManagerProfiles/azureEndpoints'
        properties: {
          targetResourceId: webMainId
          endpointStatus: 'Enabled'
        }
      }
      {
        name: 'we-web-${suffix}'
        type: 'Microsoft.Network/trafficManagerProfiles/azureEndpoints'
        properties: {
          targetResourceId: webFailoverId
          endpointStatus: 'Enabled'
        }
      }      
    ]    
  }      
}

