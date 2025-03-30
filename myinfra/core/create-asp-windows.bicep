@description('App Service Plan Name')
@minLength(3)
@maxLength(63)
param aspName string

@description('App Service Plan SKU Name')
@allowed(
  [
   'F1'
   'B1'
   'S1'
   'S2'
   'P0V3'
  ])
param skuName string

resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: aspName
  location: resourceGroup().location
  sku: {
    name: skuName
    capacity: 1
  }
}

output asp object = {
  name: appServicePlan.name
  location: appServicePlan.location
  sku: appServicePlan.sku
  id: appServicePlan.id
}
