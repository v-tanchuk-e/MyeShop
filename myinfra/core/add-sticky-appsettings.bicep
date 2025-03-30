param appServiceName string
param stickyAppSettingNames array

resource appService 'Microsoft.Web/sites@2024-04-01' existing = {
  name: appServiceName
}

resource appServiceUpdate 'Microsoft.Web/sites/config@2024-04-01' = {
  parent: appService
  name: 'slotConfigNames'
  dependsOn: [
    appService
  ]
  properties: {
    appSettingNames: stickyAppSettingNames
  }
}
