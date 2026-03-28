param environment string
param location string = deployment().location
param adminUserIds string
param aspNetEnvironment string

param containerEnvironmentId string
param registryUsername string
@secure()
param registryPassword string
param imageTag string

param authClientId string
@secure()
param authClientSecret string

param aiLocation string = location
param aiModelName string
param aiModelVersion string
param aiModelDeploymentName string

targetScope = 'subscription'

var adminUserIdList = split(adminUserIds, ',')

resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
	name: 'rg-horscht-${environment}'
	location: location
}

module Storage 'storage.bicep' = {
	name: 'Storage'
  scope: rg
	params: {
		environment: environment
		location: location
		adminUsers: adminUserIdList
	}
}

module OpenAI 'openai.bicep' = {
	scope: rg
	name: 'OpenAI'
	params: {
		environment: environment
		location: aiLocation
		modelName: aiModelName
		modelVersion: aiModelVersion
		modelDeploymentName: aiModelDeploymentName
	}
}

module Importer 'importer.bicep' = {
	scope: rg
	name: 'Importer'
	params: {
		environment: environment
		location: location
		aspNetEnvironment: aspNetEnvironment

		containerEnvironmentId: containerEnvironmentId
		imageTag: imageTag
		registryUsername: registryUsername
		registryPassword: registryPassword

		storageconnectionString: Storage.outputs.connectionString
		importQueueName: Storage.outputs.importQueueName

		authClientId: authClientId
		authClientSecret: authClientSecret

		aiEndpoint: OpenAI.outputs.endpoint
		aiApiKey: OpenAI.outputs.apiKey
		aiDeploymentName: OpenAI.outputs.deploymentName
	}
}

module Api 'api.bicep' = {
	scope: rg
	name: 'Api'
	params: {
		environment: environment
		location: location
		aspNetEnvironment: aspNetEnvironment

		containerEnvironmentId: containerEnvironmentId
		imageTag: imageTag
		registryUsername: registryUsername
		registryPassword: registryPassword

		storageconnectionString: Storage.outputs.connectionString

		authClientId: authClientId
		authClientSecret: authClientSecret
	}
}

output resourceGroupName string = rg.name
output importerAppName string = Importer.outputs.appName
output apiAppName string = Api.outputs.appName
