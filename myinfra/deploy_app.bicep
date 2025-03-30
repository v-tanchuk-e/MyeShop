targetScope = 'resourceGroup'

param appServiceName string
param gitUrl string = 'https://github.com/vive-there/eShopOnWeb.git'
param branch string = 'main'

param epoch string = '${dateTimeToEpoch(dateTimeAdd(utcNow(), 'P1Y'))}'
var suffix = uniqueString(resourceGroup().id, epoch)

module webApiDeployment './core/appservice-externalgit-manualintegration-deployment.bicep' = {
  name: 'api-depl-${suffix}'
  scope: resourceGroup()
  params:{
    appServiceName: appServiceName
    repoURL: gitUrl
    branch: branch
  }
}
