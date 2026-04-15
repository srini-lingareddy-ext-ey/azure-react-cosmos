// WO-41: Azure Event Hubs namespace (Standard tier, Kafka surface enabled) with 4 event hubs,
// consumer groups, and RBAC role assignments for the API App Service managed identity.

@minLength(1)
param environmentName string
@minLength(1)
param location string
param tags object = {}

@description('Event Hubs namespace name override.')
param eventHubsNamespaceName string = ''

@description('Principal ID of the API App Service managed identity for RBAC.')
param apiPrincipalId string

@description('Partition count per event hub. Minimum 4 for prod, 2 acceptable for dev/staging.')
param partitionCount int = 4

@description('Message retention in days. 1 for dev, 7 for staging/prod.')
param messageRetentionDays int = 7

var abbrs = loadJsonContent('../abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var namespaceName = !empty(eventHubsNamespaceName) ? eventHubsNamespaceName : '${abbrs.eventHubNamespaces}${resourceToken}'

var hubNames = [
  'pipeline-events'
  'job-events'
  'quality-events'
  'infrastructure-events'
  'lineage-analysis-requests'
]

// Azure Event Hubs Data Sender: 2b629674-e913-4c01-ae53-ef4638d8f975
// Azure Event Hubs Data Receiver: a638d3c7-ab3a-418d-83e6-5f17a39d4fde
var eventHubsDataSenderRoleId = '2b629674-e913-4c01-ae53-ef4638d8f975'
var eventHubsDataReceiverRoleId = 'a638d3c7-ab3a-418d-83e6-5f17a39d4fde'

resource namespace 'Microsoft.EventHub/namespaces@2024-01-01' = {
  name: namespaceName
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 1
  }
  properties: {
    isAutoInflateEnabled: false
    kafkaEnabled: true
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
  }
}

resource eventHubs 'Microsoft.EventHub/namespaces/eventhubs@2024-01-01' = [
  for hubName in hubNames: {
    parent: namespace
    name: hubName
    properties: {
      partitionCount: partitionCount
      messageRetentionInDays: messageRetentionDays
    }
  }
]

resource consumerGroups 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2024-01-01' = [
  for (hubName, i) in hubNames: {
    parent: eventHubs[i]
    name: 'api-consumer'
    properties: {}
  }
]

resource senderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(namespace.id, apiPrincipalId, eventHubsDataSenderRoleId)
  scope: namespace
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', eventHubsDataSenderRoleId)
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource receiverRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(namespace.id, apiPrincipalId, eventHubsDataReceiverRoleId)
  scope: namespace
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', eventHubsDataReceiverRoleId)
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output namespaceName string = namespace.name
output fullyQualifiedNamespace string = '${namespace.name}.servicebus.windows.net'
