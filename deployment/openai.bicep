param environment string
param location string = resourceGroup().location
param modelName string
param modelVersion string
param modelDeploymentName string

resource openAIAccount 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: 'oai-horscht-${environment}'
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: 'oai-horscht-${environment}'
    publicNetworkAccess: 'Enabled'
  }
}

resource modelDeployment 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  name: modelDeploymentName
  parent: openAIAccount
  sku: {
    name: 'Standard'
    capacity: 10 // Capacity in thousands of tokens per minute (TPM); 10 = 10,000 TPM
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: modelName
      version: modelVersion
    }
  }
}

output endpoint string = openAIAccount.properties.endpoint
@secure()
output apiKey string = openAIAccount.listKeys().key1
output deploymentName string = modelDeployment.name
