param environment string
param location string = resourceGroup().location
param shortLocation string
param adminUsers string[]

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
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