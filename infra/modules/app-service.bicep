// App Service Plan, Linux Web App (React frontend), and App Service (API).
// Web is deployed first so API CORS / API_ALLOW_ORIGINS can use the web hostname.

@minLength(1)
param environmentName string
@minLength(1)
param location string
param tags object = {}

@description('Application Insights resource ID for diagnostics.')
param applicationInsightsResourceId string

@description('Key Vault name for Key Vault endpoint in app settings.')
param keyVaultName string = ''

@description('App Service Plan SKU: B1 for dev, S1 for staging/prod.')
param appServicePlanSku object = { name: 'B1', tier: 'Basic', capacity: 1 }

@description('API App Service name override.')
param appServiceName string = ''

@description('Web App Service name override (React SPA).')
param webAppServiceName string = ''

@description('Additional app settings for the API (Key Vault references can use @Microsoft.KeyVault(VaultName=...)).')
param additionalAppSettings object = {}

@description('Redis connection string for ASP.NET Core ConnectionStrings:Redis (maps to ConnectionStrings__Redis app setting).')
@secure()
param redisConnectionString string

param allowedOrigins array = []

var abbrs = loadJsonContent('../abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var planName = '${abbrs.webServerFarms}${resourceToken}'
var apiSiteName = !empty(appServiceName) ? appServiceName : '${abbrs.webSitesAppService}api-${resourceToken}'
var webSiteName = !empty(webAppServiceName) ? webAppServiceName : '${abbrs.webSitesAppService}web-${resourceToken}'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: planName
  location: location
  tags: tags
  sku: appServicePlanSku
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// Frontend: Linux Node — azd deploy publishes Vite build to wwwroot; pm2 serves SPA
resource webApp 'Microsoft.Web/sites@2022-09-01' = {
  name: webSiteName
  location: location
  tags: union(tags, { 'azd-service-name': 'web' })
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'NODE|20-lts'
      alwaysOn: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appCommandLine: 'pm2 serve /home/site/wwwroot --no-daemon --spa'
    }
  }
}

resource webAppSettings 'Microsoft.Web/sites/config@2022-09-01' = {
  parent: webApp
  name: 'appsettings'
  properties: {
    APPLICATIONINSIGHTS_CONNECTION_STRING: reference(applicationInsightsResourceId, '2020-02-02').ConnectionString
    APPINSIGHTS_INSTRUMENTATIONKEY: reference(applicationInsightsResourceId, '2020-02-02').InstrumentationKey
    ApplicationInsightsAgent_EXTENSION_VERSION: '~3'
    ENABLE_ORYX_BUILD: 'true'
    SCM_DO_BUILD_DURING_DEPLOYMENT: 'false'
  }
}

resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: apiSiteName
  location: location
  tags: union(tags, { 'azd-service-name': 'api' })
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      cors: {
        allowedOrigins: union(
          [
            'https://portal.azure.com'
            'https://ms.portal.azure.com'
            'https://${webApp.properties.defaultHostName}'
          ],
          allowedOrigins
        )
        supportCredentials: true
      }
    }
  }
}

// API app settings (Application Insights + Key Vault + Cosmos + Redis connection string + CORS origin for web App Service)
resource appSettingsResource 'Microsoft.Web/sites/config@2022-09-01' = {
  parent: appService
  name: 'appsettings'
  properties: union(
    {
      APPLICATIONINSIGHTS_CONNECTION_STRING: reference(applicationInsightsResourceId, '2020-02-02').ConnectionString
      APPINSIGHTS_INSTRUMENTATIONKEY: reference(applicationInsightsResourceId, '2020-02-02').InstrumentationKey
      ApplicationInsightsAgent_EXTENSION_VERSION: '~3'
      AZURE_KEY_VAULT_ENDPOINT: 'https://${keyVaultName}.${environment().suffixes.keyvaultDns}'
      ENABLE_ORYX_BUILD: 'true'
      SCM_DO_BUILD_DURING_DEPLOYMENT: 'false'
      API_ALLOW_ORIGINS: 'https://${webApp.properties.defaultHostName}'
      ConnectionStrings__Redis: redisConnectionString
    },
    additionalAppSettings
  )
}

output appServiceName string = appService.name
output webAppName string = webApp.name
output appServicePlanName string = appServicePlan.name
output principalId string = appService.identity.principalId
output defaultHostname string = appService.properties.defaultHostName
output webDefaultHostname string = webApp.properties.defaultHostName
output resourceId string = appService.id
output webResourceId string = webApp.id
