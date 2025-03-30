param appServiceName string
param aspPlanName string

resource appService 'Microsoft.Web/sites@2024-04-01' = {
  name: appServiceName
  location: resourceGroup().location
  properties: {
    httpsOnly: true
    serverFarmId: resourceId('Microsoft.Web/serverfarms', aspPlanName)
  }
}


output app object = {
  name: appService.name
  defaultHostName: appService.properties.defaultHostName
  id: appService.id
  currentAppSettings: list('${appService.id}/config/appsettings', '2024-04-01').properties
}
