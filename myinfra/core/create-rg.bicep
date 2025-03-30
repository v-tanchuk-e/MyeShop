targetScope = 'subscription'

@description('Resource group location')
@allowed(['westeurope','northeurope'])
param location string

@description('Resource group name') 
@minLength(1)
@maxLength(90)
param resourceGroupName string

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-11-01' = {
  name: resourceGroupName
  location: location
}

output rg  object = {
  name: resourceGroup.name
  location: resourceGroup.location
}
