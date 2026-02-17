param location string = resourceGroup().location
param appName string = 'infinity-ecom'

// 1. SQL Server & DB
resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: '${appName}-sql'
  location: location
  properties: {
    administratorLogin: 'adminUser'
    administratorLoginPassword: 'SecurePassword123!' 
  }
}
resource sqlDb 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent: sqlServer
  name: 'InfinityDb'
  location: location
  sku: { name: 'Basic', tier: 'Basic' }
}

// 2. Redis Cache
resource redis 'Microsoft.Cache/redis@2023-04-01' = {
  name: '${appName}-redis'
  location: location
  properties: {
    sku: { name: 'Basic', family: 'C', capacity: 0 }
  }
}

// 3. App Service Plan (Linux)
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: '${appName}-plan'
  location: location
  sku: { name: 'B1' }
  kind: 'linux'
  properties: { reserved: true }
}

// 4. Web App (API)
resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: '${appName}-api'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      appSettings: [
        { name: 'ConnectionStrings__Redis', value: '${redis.properties.hostName},password=${redis.properties.accessKeys.primaryKey},ssl=True,abortConnect=False' }
      ]
    }
  }
}