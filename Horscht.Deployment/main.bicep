param environment string
param location string = deployment().location
param adminUserIds string

targetScope = 'subscription'

var shortLocation = location == 'westeurope' ? 'weu' : location == 'germanywestcentral' ? 'gwc' : location

var adminUserIdList = split(adminUserIds, ',')

resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
	name: 'rg-horscht-${shortLocation}-${environment}'
	location: location
}

module Storage 'storage.bicep' = {
	name: 'Storage'
  scope: rg
	params: {
		environment: environment
		location: location
		shortLocation: shortLocation
		adminUsers: adminUserIdList
	}
}