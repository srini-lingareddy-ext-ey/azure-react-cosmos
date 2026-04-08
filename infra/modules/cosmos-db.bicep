// Cosmos DB account module. Primary data store with serverless or provisioned throughput.
// AC-FOUNDATION-019.2: Modular Bicep for each Azure service; backup and security per Infrastructure blueprint.

@minLength(1)
param environmentName string
@minLength(1)
param location string
param tags object = {}

@allowed([ 'Session', 'Eventual', 'ConsistentPrefix', 'Strong', 'BoundedStaleness' ])
@description('Consistency level for the Cosmos DB account.')
param consistencyLevel string = 'Session'

@description('Cosmos DB account name override.')
param cosmosAccountName string = ''

@description('Database name.')
param databaseName string = 'App'

@description('Container name.')
param containerName string = 'Items'

@description('Principal ID for Cosmos DB Data Contributor (e.g. API managed identity).')
param principalId string = ''

var abbrs = loadJsonContent('../abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var accountName = !empty(cosmosAccountName) ? cosmosAccountName : '${abbrs.documentDBDatabaseAccounts}${resourceToken}'

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: accountName
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: { defaultConsistencyLevel: consistencyLevel }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    enableAutomaticFailover: false
    disableLocalAuth: true
    capabilities: [{ name: 'EnableServerless' }]
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 240
        backupRetentionIntervalInHours: 8
        backupStorageRedundancy: 'Geo'
      }
    }
    apiProperties: {}
  }
}

resource sqlDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: cosmosAccount
  name: databaseName
  properties: {
    resource: { id: databaseName }
  }
}

resource container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: sqlDatabase
  name: containerName
  properties: {
    resource: {
      id: containerName
      partitionKey: { paths: ['/id'], kind: 'Hash' }
    }
  }
}

// WO-4: tenant configuration container (partition /id)
resource tenantContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: sqlDatabase
  name: 'tenant'
  properties: {
    resource: {
      id: 'tenant'
      partitionKey: { paths: ['/id'], kind: 'Hash' }
    }
  }
}

// WO-5: user role assignments per tenant (partition /tenantId)
resource userRoleAssignmentContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: sqlDatabase
  name: 'user-role-assignment'
  properties: {
    resource: {
      id: 'user-role-assignment'
      partitionKey: { paths: ['/tenantId'], kind: 'Hash' }
    }
  }
}

// Cosmos DB Built-in Data Contributor role for the API principal
resource roleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-05-15' = if (!empty(principalId)) {
  parent: cosmosAccount
  name: guid(principalId, accountName, '00000000-0000-0000-0000-000000000002')
  properties: {
    roleDefinitionId: '${cosmosAccount.id}/sqlRoleDefinitions/00000000-0000-0000-0000-000000000002'
    principalId: principalId
    scope: cosmosAccount.id
  }
}

output accountName string = cosmosAccount.name
output endpoint string = cosmosAccount.properties.documentEndpoint
output databaseName string = databaseName
output resourceId string = cosmosAccount.id
