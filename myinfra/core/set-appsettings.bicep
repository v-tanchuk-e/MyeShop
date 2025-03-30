@description('The name of the Azure App Service')
param appServiceName string

@description('Array of appsettings')
param appSettings object
param currentAppSettings object

resource appService 'Microsoft.Web/sites@2024-04-01' existing = {
  name: appServiceName
}

resource appServiceUpdate 'Microsoft.Web/sites/config@2024-04-01' = {
  parent: appService
  name: 'appsettings'
  properties: union( currentAppSettings , appSettings)
}
