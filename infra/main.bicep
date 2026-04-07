// Main Bicep orchestration - coordinates all resource deployments per AC-FOUNDATION-019.1.
// Uses abbreviations.json for naming; deploys App Service Plan + Web (React) + API, Cosmos DB,
// Key Vault, Redis, Application Insights, RBAC, and optional APIM.

targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment used to generate a short unique hash for all resources.')
param environmentName string

@minLength(1)
@description('Primary location for all resources.')
param location string

@description('Principal ID of the user or app to assign application roles (e.g. for Key Vault secret seeding).')
param principalId string = ''

// Optional overrides for resource names (set in main.parameters.json or env-specific parameter files)
param resourceGroupName string = ''
param applicationInsightsName string = ''
param logAnalyticsName string = ''
param keyVaultName string = ''
param cosmosAccountName string = ''
param appServiceName string = ''
param webAppServiceName string = ''
param redisName string = ''
param apimServiceName string = ''

@description('Use Azure API Management to mediate calls between frontend and backend API.')
param useAPIM bool = false

@description('API Management SKU when APIM is enabled (e.g. Consumption, Developer).')
param apimSku string = 'Consumption'

@description('App Service Plan SKU: B1 for dev, S1 for staging/prod.')
param appServicePlanSku object = { name: 'B1', tier: 'Basic', capacity: 1 }

@description('Redis SKU: Basic for dev, Standard for staging/prod.')
param redisSku string = 'Basic'

var abbrs = loadJsonContent('./abbreviations.json')
var tags = {
  'azd-env-name': environmentName
  Environment: environmentName
  Application: 'todo-csharp-cosmos-sql'
  ManagedBy: 'bicep'
}

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: !empty(resourceGroupName) ? resourceGroupName : '${abbrs.resourcesResourceGroups}${environmentName}'
  location: location
  tags: tags
}

// Application Insights and Log Analytics (no dependencies)
module monitoring './modules/app-insights.bicep' = {
  name: 'monitoring'
  scope: rg
  params: {
    environmentName: environmentName
    location: location
    tags: tags
    applicationInsightsName: !empty(applicationInsightsName) ? applicationInsightsName : ''
    logAnalyticsName: !empty(logAnalyticsName) ? logAnalyticsName : ''
  }
}

// Key Vault with RBAC (no dependency on App Service)
module keyVault './modules/key-vault.bicep' = {
  name: 'keyvault'
  scope: rg
  params: {
    environmentName: environmentName
    location: location
    tags: tags
    keyVaultName: !empty(keyVaultName) ? keyVaultName : ''
    enablePurgeProtection: true
  }
}

// Cosmos DB (no principalId yet; role assignment in separate step)
module cosmos './modules/cosmos-db.bicep' = {
  name: 'cosmos'
  scope: rg
  params: {
    environmentName: environmentName
    location: location
    tags: tags
    cosmosAccountName: !empty(cosmosAccountName) ? cosmosAccountName : ''
    principalId: '' // Set in cosmos-role-assignment after API exists
  }
}

// Azure Cache for Redis — must exist before API App Service so ConnectionStrings__Redis can be set
module redis './modules/redis.bicep' = {
  name: 'redis'
  scope: rg
  params: {
    environmentName: environmentName
    location: location
    tags: tags
    sku: redisSku
    capacity: redisSku == 'Basic' ? 0 : 1
    redisName: !empty(redisName) ? redisName : ''
  }
}

// App Service Plan + Web App (React) + API App Service (CORS / API_ALLOW_ORIGINS use web hostname)
module appService './modules/app-service.bicep' = {
  name: 'api-appservice'
  scope: rg
  params: {
    environmentName: environmentName
    location: location
    tags: tags
    applicationInsightsResourceId: monitoring.outputs.applicationInsightsResourceId
    keyVaultName: keyVault.outputs.name
    appServicePlanSku: appServicePlanSku
    appServiceName: !empty(appServiceName) ? appServiceName : ''
    webAppServiceName: !empty(webAppServiceName) ? webAppServiceName : ''
    redisConnectionString: redis.outputs.connectionString
    additionalAppSettings: {
      AZURE_COSMOS_ENDPOINT: cosmos.outputs.endpoint
      AZURE_COSMOS_DATABASE_NAME: cosmos.outputs.databaseName
    }
    allowedOrigins: []
  }
}

// Cosmos DB Built-in Data Contributor for API managed identity
module apiCosmosRole './app/cosmos-role-assignment.bicep' = {
  name: 'api-cosmos-role'
  scope: rg
  params: {
    cosmosAccountName: cosmos.outputs.accountName
    apiPrincipalId: appService.outputs.principalId
  }
}

// RBAC: Key Vault Secrets User for API, Secrets Officer for deployment principal
module rbac './modules/rbac.bicep' = {
  name: 'rbac'
  scope: rg
  params: {
    appServicePrincipalId: appService.outputs.principalId
    deploymentPrincipalId: principalId
    keyVaultName: keyVault.outputs.name
  }
}

// Optional APIM
module apim './modules/apim.bicep' = if (useAPIM) {
  name: 'apim'
  scope: rg
  params: {
    environmentName: environmentName
    location: location
    tags: tags
    apimSku: apimSku
    apimServiceName: !empty(apimServiceName) ? apimServiceName : ''
  }
}

// Outputs for azd and application configuration
output AZURE_COSMOS_ENDPOINT string = cosmos.outputs.endpoint
output AZURE_COSMOS_DATABASE_NAME string = cosmos.outputs.databaseName
output APPLICATIONINSIGHTS_CONNECTION_STRING string = monitoring.outputs.applicationInsightsConnectionString
output AZURE_KEY_VAULT_ENDPOINT string = keyVault.outputs.uri
output AZURE_KEY_VAULT_NAME string = keyVault.outputs.name
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output API_BASE_URL string = useAPIM ? apim!.outputs.gatewayUrl : 'https://${appService.outputs.defaultHostname}'
output REACT_APP_WEB_BASE_URL string = 'https://${appService.outputs.webDefaultHostname}'
output USE_APIM bool = useAPIM
output SERVICE_API_ENDPOINTS array = useAPIM ? [ apim!.outputs.gatewayUrl, 'https://${appService.outputs.defaultHostname}' ] : []
output REDIS_HOST string = redis.outputs.hostName
output REDIS_PORT int = redis.outputs.sslPort
