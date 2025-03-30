param appServiceName string
param slotName string
param appSettings array

resource appService 'Microsoft.Web/sites@2024-04-01' existing = {
  name: appServiceName
}

resource appServiceSlot 'Microsoft.Web/sites/slots@2024-04-01' = {
  name: slotName
  parent: appService
  location: resourceGroup().location
  kind: 'app'
  properties: {
    serverFarmId: appService.properties.serverFarmId
    siteConfig: {
      appSettings: appSettings
    }
  }
}

resource appServiceUpdate 'Microsoft.Web/sites/config@2024-04-01' = {
  parent: appService
  name: 'slotConfigNames'
  properties: {
    appSettingNames: [
      'baseUrls__apiBase'
      'baseUrls__webBase'
      'UseOnlyInMemoryDatabase'
      'WEBSITE_RUN_FROM_PACKAGE'
      'SCM_DO_BUILD_DURING_DEPLOYMENT'
    ]
  }
  dependsOn: [
    appServiceSlot
  ]
}


output appServiceSlotOut object = {
  name: appServiceSlot.name
  id: appServiceSlot.id
  defaultHostName: appServiceSlot.properties.defaultHostName
}
