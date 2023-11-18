param environment string
param location string = resourceGroup().location
param shortLocation string
param adminUsers string[]

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'sthorscht${shortLocation}${environment}'
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