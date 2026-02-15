param environment string
param location string = resourceGroup().location
param aspNetEnvironment string

param containerEnvironmentId string
param containerEnvironmentResourceGroupName string
param containerEnvironmentName string
param registryUsername string
@secure()
param registryPassword string
param imageTag string
param hostname string

@secure()
param storageconnectionString string

param authClientId string
@secure()
param authClientSecret string

var environmentVariables = [
  {
    name: 'Storage__ConnectionString'
    secretRef: 'storage-connectionstring'
  }
  {
    name: 'AzureAd__TenantId'
    value: tenant().tenantId
  }
  {
    name: 'AzureAd__ClientId'
    value: authClientId
  }
  {
    name: 'AzureAd__ClientSecret'
    secretRef: 'auth-client-secret'
  }
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: aspNetEnvironment
  }
]

var providedSecrets = [
  {
    name: 'registry-password'
    value: registryPassword
  }
  {
    name: 'storage-connectionstring'
    value: storageconnectionString
  }
  {
    name: 'auth-client-secret'
    value: authClientSecret
  }
]

resource containerApp 'Microsoft.App/containerApps@2023-11-02-preview' = {
  name: 'aca-horscht-api-${environment}'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerEnvironmentId
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
        customDomains: [
          {
            name: hostname
            bindingType: 'Disabled'
          }
        ]
      }
      registries: [
        {
          server: 'ghcr.io'
          username: registryUsername
          passwordSecretRef: 'registry-password'
        }
      ]
      secrets: providedSecrets
    }
    template: {
      containers: [
        {
          image: 'ghcr.io/thomasneuberger/horscht-api:${imageTag}'
          name: 'api'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: environmentVariables
          probes: [
            {
              type: 'Liveness'
              initialDelaySeconds: 15
              httpGet: {
                port: 8080
                path: 'api/health'
              }
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 10
        rules: [
          {
            name: 'http-rule'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
}

resource environmentResourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' existing = {
  name: containerEnvironmentResourceGroupName
  scope: subscription()
}

module Certificate 'managedCertificate.bicep' = {
  name: 'ApiCertificate'
  scope: environmentResourceGroup
  params: {
    location: location
    containerEnvironmentName: containerEnvironmentName
    hostname: hostname
  }
}

output appName string = containerApp.name
output certificateId string = Certificate.outputs.id
