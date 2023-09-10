param environment string
param location string = resourceGroup().location
param shortLocation string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: 'sthorscht${shortLocation}${environment}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}
