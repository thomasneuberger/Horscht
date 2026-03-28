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
    [string] $registryUsername,
    [string] $registryPassword,
    [string] $imageTag,
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
                           registryUsername=$registryUsername `
                           registryPassword=$registryPassword `
                           imageTag=$imageTag `
                           authClientId=$authClientId `
                           authClientSecret=$authClientSecret `
                           aiModelName=$aiModelName `
                           aiModelVersion=$aiModelVersion `
                           aiModelDeploymentName=$aiModelDeploymentName `
                         --subscription $Subscription

$output = $outputJson | ConvertFrom-Json

$resourceGroupName = $output.properties.outputs.resourceGroupName.value

$importerAppName = $output.properties.outputs.importerAppName.value

$apiAppName = $output.properties.outputs.apiAppName.value

