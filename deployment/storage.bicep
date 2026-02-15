param environment string
param location string = resourceGroup().location
param adminUsers string[]

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'sthorscht${environment}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  name: 'default'
  parent: storageAccount
  properties: {
    cors: {
      corsRules: [
        {
          allowedHeaders: [
            '*'
          ]
          allowedMethods: [
            'GET'
            'POST'
            'PUT'
            'DELETE'
          ]
          allowedOrigins: [
            'https://localhost:7043'
            'https://localhost'
          ]
          exposedHeaders: []
          maxAgeInSeconds: 60
        }
      ]
    }
  }
}

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2023-01-01' = {
  name: 'default'
  parent: storageAccount
  properties: {
    cors: {
      corsRules: [
        {
          allowedHeaders: [
            '*'
          ]
          allowedMethods: [
            'GET'
            'POST'
            'PUT'
          ]
          allowedOrigins: [
            'https://localhost:7043'
            'https://localhost'
          ]
          exposedHeaders: []
          maxAgeInSeconds: 60
        }
      ]
    }
  }
}

resource importQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = {
  name: 'import'
  parent: queueService
}

module BuiltInRoles 'builtInRoles.bicep' = {
  name: 'StorageBuiltInRoles'
}

resource adminBlobPermissions 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = [for user in adminUsers: {
  name: guid('horscht', environment, 'storage', 'blob', 'admin', user)
  properties: {
    roleDefinitionId: BuiltInRoles.outputs.StorageBlobDataContributor
    principalId: user
    principalType: 'User'
  }
}]

resource adminQueuePermissions 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = [for user in adminUsers: {
  name: guid('horscht', environment, 'storage', 'queue', 'admin', user)
  properties: {
    roleDefinitionId: BuiltInRoles.outputs.StorageQueueDataContributor
    principalId: user
    principalType: 'User'
  }
}]

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  name: 'default'
  parent: storageAccount
  properties: {
    cors: {
      corsRules: [
        {
          allowedHeaders: [
            '*'
          ]
          allowedMethods: [
            'GET'
            'POST'
            'PUT'
            'DELETE'
          ]
          allowedOrigins: [
            'https://localhost:7043'
            'https://localhost'
          ]
          exposedHeaders: []
          maxAgeInSeconds: 60
        }
      ]
    }
  }
}

resource songTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-01-01' = {
	name: 'songs'
  parent: tableService
}

resource adminTablePermissions 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = [for user in adminUsers: {
  name: guid('horscht', environment, 'storage', 'table', 'admin', user)
  properties: {
    roleDefinitionId: BuiltInRoles.outputs.StorageTableDataContributor
    principalId: user
    principalType: 'User'
  }
}]

var accountKey = storageAccount.listKeys(storageAccount.apiVersion).keys[0].value
output connectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${accountKey};EndpointSuffix=${az.environment().suffixes.storage}'

output importQueueName string = 'https://${storageAccount.name}.queue.${az.environment().suffixes.storage}/${importQueue.name}'
