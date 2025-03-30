@description('The name of the Azure App Service')
param appServiceName string

@description('The URL for the GitHub repository that contains the project to deploy.')
param repoURL string

@description('The branch of the GitHub repository to use.')
param branch string

// Get existing app service
resource appService 'Microsoft.Web/sites@2024-04-01' existing = {
  name: appServiceName
}

// AND then setup the deployment
resource appServiceDeployment 'Microsoft.Web/sites/sourcecontrols@2024-04-01' = {
  parent: appService
  name: 'web'
  properties: {
    isManualIntegration: true
    branch: branch
    repoUrl: repoURL
  }
}

