#Requires -Version 3.0

Param(
    [Parameter(Mandatory=$true)]
    [string] $Subscription,
    [Parameter(Mandatory=$true)]
    [string] $Environment,
    [Parameter(Mandatory=$true)]
    [string] $Location,
    [string] $aspNetEnvironment,
    [string] $adminUserIds = "",
    [string] $containerEnvironmentId,
    [string] $containerEnvironmentResourceGroupName,
    [string] $containerEnvironmentName,
    [string] $registryUsername,
    [string] $registryPassword,
    [string] $imageTag,
    [string] $importerHostname,
    [string] $apiHostname,
    [string] $authClientId,
    [string] $authClientSecret,
    [string] $aiModelName,
    [string] $aiModelVersion,
    [string] $aiModelDeploymentName
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3

$outputJson = az deployment sub create --location $Location `
                         --name Horscht `
                         --template-file main.bicep `
                         --parameters `
                           environment=$Environment `
                           location=$Location `
                           aspNetEnvironment=$aspNetEnvironment `
                           adminUserIds=$adminUserIds `
                           containerEnvironmentId=$containerEnvironmentId `
                           containerEnvironmentResourceGroupName=$containerEnvironmentResourceGroupName `
                           containerEnvironmentName=$containerEnvironmentName `
                           registryUsername=$registryUsername `
                           registryPassword=$registryPassword `
                           imageTag=$imageTag `
                           importerHostname=$importerHostname `
                           apiHostname=$apiHostname `
                           authClientId=$authClientId `
                           authClientSecret=$authClientSecret `
                           aiModelName=$aiModelName `
                           aiModelVersion=$aiModelVersion `
                           aiModelDeploymentName=$aiModelDeploymentName `
                         --subscription $Subscription

$output = $outputJson | ConvertFrom-Json

$resourceGroupName = $output.properties.outputs.resourceGroupName.value

$importerAppName = $output.properties.outputs.importerAppName.value
$importerCertificateId = $output.properties.outputs.importerCertificateId.value

$apiAppName = $output.properties.outputs.apiAppName.value
$apiCertificateId = $output.properties.outputs.apiCertificateId.value

az containerapp hostname bind --subscription $Subscription -g $resourceGroupName -n $importerAppName --hostname $importerHostname -c $importerCertificateId | Out-Null
az containerapp hostname bind --subscription $Subscription -g $resourceGroupName -n $apiAppName --hostname $apiHostname -c $apiCertificateId | Out-Null