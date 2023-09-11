#Requires -Version 3.0

Param(
    [Parameter(Mandatory=$true)]
    [string] $Subscription,
    [Parameter(Mandatory=$true)]
    [string] $Environment,
    [Parameter(Mandatory=$true)]
    [string] $Location,
    [string] $adminUserIds = ""
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3

az deployment sub create --location $Location `
                         --name Horscht `
                         --template-file main.bicep `
                         --parameters `
                           environment=$Environment `
                           location=$Location `
                           adminUserIds=$adminUserIds `
                         --subscription $Subscription
