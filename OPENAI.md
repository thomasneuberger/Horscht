# Azure OpenAI Setup

This document describes how to set up Azure OpenAI for use with the Horscht Importer service.

## Overview

The Horscht Importer uses Azure OpenAI to enable AI-powered features during music file processing. The Azure OpenAI service and model are provisioned alongside the rest of the infrastructure using Bicep templates.

## Prerequisites

### Request Azure OpenAI Access

Azure OpenAI is a limited-access service. Before you can deploy the service you must request and receive access:

1. Go to [https://aka.ms/oai/access](https://aka.ms/oai/access) and fill in the request form.
2. Wait for an approval email from Microsoft (typically within a few business days).
3. Once approved, the Azure OpenAI service type will be available in your Azure subscription.

### Verify Model Availability

Not every Azure OpenAI model is available in every Azure region. Before deploying:

1. Check the [Azure OpenAI model availability page](https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models#model-summary-table-and-region-availability) to confirm your chosen model is available in the target region.
2. Use `aiLocation` to deploy the OpenAI service to a region that supports your model (see [Deploying to a Different Region](#deploying-to-a-different-region) below).

## Infrastructure Deployment

The Azure OpenAI service and model deployment are defined in `deployment/openai.bicep` and provisioned by `deployment/main.bicep`.

### Bicep Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `aiLocation` | Azure region for the OpenAI service | Same as `location` (the app region) |
| `aiModelName` | OpenAI model name to deploy (e.g. `gpt-4o`) | *(required)* |
| `aiModelVersion` | Model version to deploy (e.g. `2024-11-20`) | *(required)* |
| `aiModelDeploymentName` | Name for the model deployment (e.g. `gpt-4o`) | *(required)* |

### Deploying to a Different Region

Azure OpenAI model availability varies by region. You can deploy the OpenAI service to a different region than the rest of the application by setting `aiLocation`:

```powershell
az deployment sub create `
  --location westeurope `
  --template-file deployment/main.bicep `
  --parameters environment=prod `
               location=westeurope `
               aiLocation=swedencentral `
               aiModelName=gpt-4o `
               aiModelVersion=2024-11-20 `
               aiModelDeploymentName=gpt-4o `
               ...
```

If `aiLocation` is not specified, the OpenAI service is deployed to the same region as the application.

## GitHub Actions Workflow Variables

The model name, version, and deployment name can be configured using GitHub Actions [repository variables](https://docs.github.com/en/actions/writing-workflows/choosing-what-your-workflow-does/store-information-in-variables) so they are shared across workflow runs without modifying the workflow file.

### Setting Repository Variables

1. Go to your GitHub repository → **Settings** → **Secrets and variables** → **Actions** → **Variables** tab.
2. Add the following variables:

| Variable | Example value | Description |
|----------|---------------|-------------|
| `AI_MODEL_NAME` | `gpt-4o` | Azure OpenAI model name |
| `AI_MODEL_VERSION` | `2024-11-20` | Azure OpenAI model version |
| `AI_MODEL_DEPLOYMENT_NAME` | `gpt-4o` | Name for the model deployment |

If these variables are not set, the workflow falls back to the hardcoded defaults (`gpt-4o` / `2024-11-20`).

## Importer Configuration

The Importer service receives the OpenAI connection details as environment variables set by the Bicep deployment:

| Environment variable | Configuration key | Description |
|----------------------|-------------------|-------------|
| `AzureOpenAI__Endpoint` | `AzureOpenAI:Endpoint` | Azure OpenAI service endpoint URL |
| `AzureOpenAI__ApiKey` | `AzureOpenAI:ApiKey` | Azure OpenAI API key (stored as a Container App secret) |
| `AzureOpenAI__DeploymentName` | `AzureOpenAI:DeploymentName` | Name of the deployed model |

These map to the `AzureOpenAIOptions` class in `Horscht.Importer`:

```csharp
public class AzureOpenAIOptions
{
    public required string Endpoint { get; set; }
    public required string ApiKey { get; set; }
    public required string DeploymentName { get; set; }
}
```

### Local Development

For local development, add the following to your [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for `Horscht.Importer`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://oai-horscht-dev.openai.azure.com/",
    "ApiKey": "<your-api-key>",
    "DeploymentName": "gpt-4o"
  }
}
```

Or set the equivalent environment variables:

```bash
AzureOpenAI__Endpoint=https://oai-horscht-dev.openai.azure.com/
AzureOpenAI__ApiKey=<your-api-key>
AzureOpenAI__DeploymentName=gpt-4o
```
