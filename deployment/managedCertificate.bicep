param location string = resourceGroup().location
param containerEnvironmentName string
param hostname string

resource containerEnvironment 'Microsoft.App/managedEnvironments@2023-11-02-preview' existing = {
  name: containerEnvironmentName
}

resource certificate 'Microsoft.App/managedEnvironments/managedCertificates@2023-11-02-preview' = {
  name: replace(hostname, '.', '-')
  parent: containerEnvironment
  location: location
  properties: {
    subjectName: hostname
    domainControlValidation: 'TXT'
  }
}

output id string = certificate.id
