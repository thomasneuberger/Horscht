# Azure Deployment

This folder contains the Infrastructure as Code (IaC) templates and scripts for deploying the Horscht application to Azure.

## Files

- **main.bicep** - Main deployment template that orchestrates all resources
- **storage.bicep** - Azure Storage account configuration (Blob, Queue, Table)
- **importer.bicep** - Container App deployment for the Importer service
- **api.bicep** - Container App deployment for the API service (Backend for Frontend)
- **builtInRoles.bicep** - Azure built-in role definitions
- **managedCertificate.bicep** - Managed certificate configuration for custom domains
- **Deploy-AzureResourceGroup.ps1** - PowerShell deployment script

## Azure Resources Created

The deployment creates the following resources:

### Storage Account
- **Blob containers**: For storing uploaded music files
- **Queue**: `import` - For processing uploaded files asynchronously
- **Table**: `songs` - For storing song metadata and catalog information
- **CORS**: Configured for localhost development

### Container Apps

#### Importer Service
- Background service that processes uploaded files
- Extracts metadata from audio files
- Updates the song catalog
- Scales based on queue depth (scale to zero when idle)

#### API Service (Backend for Frontend)
- REST API for the web application
- Handles business logic and data access
- Scales based on HTTP traffic (scale to zero when idle)
- Requires separate Azure AD app registration (see [AUTHENTICATION.md](../AUTHENTICATION.md))

## Continuous Deployment

The [CD workflow](../.github/workflows/cd.yml) automatically deploys to the **dev** environment on every push to the `master` branch. It:

1. Builds and pushes Docker images to GitHub Container Registry (GHCR)
2. Deploys all Azure resources using the Bicep templates
3. Binds custom hostnames for the Importer and API container apps

### GitHub Secrets

The following secrets must be configured in the GitHub repository:

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | Client ID of the Azure AD app registration used for OIDC authentication |
| `AZURE_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |
| `DEV_ADMIN_USER_IDS` | Comma-separated list of Azure AD object IDs that receive admin access to storage |
| `DEV_REGISTRY_PASSWORD` | GitHub Personal Access Token (PAT) with `read:packages` scope for pulling images |
| `DEV_AUTH_CLIENT_SECRET` | Client secret of the Azure AD app registration used by the application |

### GitHub Variables

The following repository variables must be configured:

| Variable | Description | Example |
|----------|-------------|---------|
| `DEV_LOCATION` | Azure region for the deployment | `westeurope` |
| `DEV_CONTAINER_ENVIRONMENT_ID` | Resource ID of the existing Azure Container Apps Environment | `/subscriptions/.../containerEnvironments/ce-horscht` |
| `DEV_CONTAINER_ENVIRONMENT_RG` | Resource group of the Container Apps Environment | `rg-container-env` |
| `DEV_CONTAINER_ENVIRONMENT_NAME` | Name of the Container Apps Environment | `ce-horscht` |
| `DEV_IMPORTER_HOSTNAME` | Custom domain for the Importer container app | `importer.example.com` |
| `DEV_API_HOSTNAME` | Custom domain for the API container app | `api.example.com` |
| `DEV_AUTH_CLIENT_ID` | Client ID of the Azure AD app registration used by the application | |
| `AI_MODEL_NAME` | Azure OpenAI model name (optional, defaults to `gpt-4o`) | `gpt-4o` |
| `AI_MODEL_VERSION` | Azure OpenAI model version (optional, defaults to `2024-11-20`) | `2024-11-20` |
| `AI_MODEL_DEPLOYMENT_NAME` | Azure OpenAI deployment name (optional, defaults to `gpt-4o`) | `gpt-4o` |

### Azure OIDC Setup

The CD workflow uses [OpenID Connect (OIDC)](https://docs.github.com/en/actions/security-for-github-actions/security-hardening-your-deployments/about-security-hardening-with-openid-connect) for passwordless Azure authentication. To set this up:

1. Create an Azure AD app registration
2. Add a Federated Identity Credential scoped to the GitHub Actions environment:
   - **Organization**: `thomasneuberger`
   - **Repository**: `Horscht`
   - **Entity type**: `Environment`
   - **Environment name**: `dev`
3. Assign the app registration the `Contributor` role on your Azure subscription (or at minimum on the resource group)
4. Store the `Client ID`, `Tenant ID`, and `Subscription ID` as GitHub secrets (`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`)

## Manual Deployment

### Prerequisites
- Azure subscription
- Azure CLI installed and logged in
- Appropriate permissions to create resources

### Using PowerShell Script

```powershell
.\Deploy-AzureResourceGroup.ps1 `
    -Subscription "<subscription-id>" `
    -Environment "dev" `
    -Location "westeurope" `
    -aspNetEnvironment "Development" `
    -adminUserIds "<user-object-id>" `
    -containerEnvironmentId "<container-env-resource-id>" `
    -containerEnvironmentResourceGroupName "<container-env-rg>" `
    -containerEnvironmentName "<container-env-name>" `
    -registryUsername "<github-username>" `
    -registryPassword "<ghcr-pat>" `
    -imageTag "<image-tag>" `
    -importerHostname "<importer-hostname>" `
    -authClientId "<azure-ad-client-id>" `
    -authClientSecret "<azure-ad-client-secret>"
```

### Using Azure CLI

```bash
az deployment sub create \
    --location westeurope \
    --name Horscht \
    --template-file main.bicep \
    --parameters \
        environment=dev \
        location=westeurope \
        aspNetEnvironment=Development \
        adminUserIds="<user-object-id>" \
        containerEnvironmentId="<container-env-resource-id>" \
        containerEnvironmentResourceGroupName="<container-env-rg>" \
        containerEnvironmentName="<container-env-name>" \
        registryUsername="<github-username>" \
        registryPassword="<ghcr-pat>" \
        imageTag="<image-tag>" \
        importerHostname="<importer-hostname>" \
        apiHostname="<api-hostname>" \
        authClientId="<azure-ad-client-id>" \
        authClientSecret="<azure-ad-client-secret>" \
        aiModelName="gpt-4o" \
        aiModelVersion="2024-11-20" \
        aiModelDeploymentName="gpt-4o"
```

## Local Development

For local development, use .NET Aspire instead of deploying to Azure:
1. Run the `Horscht.AppHost` project
2. Azurite (local Azure Storage emulator) will be started automatically
3. All storage resources (queue, table) are created automatically on startup

See [ASPIRE.md](../ASPIRE.md) for more information about local development with Aspire.
