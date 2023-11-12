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
            'GET'
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

module BuiltInRoles 'builtInRoles.bicep' = {
  name: 'StorageBuiltInRoles'
}

resource adminPermissions 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = [for user in adminUsers: {
  name: guid('horscht', environment, 'storage', 'admin', user)
  properties: {
    roleDefinitionId: BuiltInRoles.outputs.StorageBlobDataContributor
    principalId: user
    principalType: 'User'
  }
}]