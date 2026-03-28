param environment string
param location string = deployment().location
param adminUserIds string
param aspNetEnvironment string

param containerEnvironmentId string
param containerEnvironmentResourceGroupName string
param containerEnvironmentName string
param registryUsername string
@secure()
param registryPassword string
param imageTag string
param importerHostname string
param apiHostname string

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
		containerEnvironmentResourceGroupName: containerEnvironmentResourceGroupName
		containerEnvironmentName: containerEnvironmentName
		imageTag: imageTag
		registryUsername: registryUsername
		registryPassword: registryPassword
		hostname: importerHostname

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
		containerEnvironmentResourceGroupName: containerEnvironmentResourceGroupName
		containerEnvironmentName: containerEnvironmentName
		imageTag: imageTag
		registryUsername: registryUsername
		registryPassword: registryPassword
		hostname: apiHostname

		storageconnectionString: Storage.outputs.connectionString

		authClientId: authClientId
		authClientSecret: authClientSecret
	}
}

output resourceGroupName string = rg.name
output importerAppName string = Importer.outputs.appName
output importerHostname string = importerHostname
output importerCertificateId string = Importer.outputs.certificateId
output apiAppName string = Api.outputs.appName
output apiHostname string = apiHostname
output apiCertificateId string = Api.outputs.certificateId
