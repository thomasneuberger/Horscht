#Requires -Version 3.0

Param(
    [string] $Subscription = "f18dfbea-b305-4007-b32d-b1fc064d1ff1",
    [string] $Environment = "dev",
    [string] $Location = "germanywestcentral",
    [string] $TemplateFile = 'main.bicep',
    [string] $TemplateParametersFile = 'azuredeploy.parameters.json'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3

az deployment sub create --location $Location `
                         --name 'Horscht' `
                         --template-file $TemplateFile `
                         --parameters environment=$Environment location=$Location `
                         --subscription $Subscription
