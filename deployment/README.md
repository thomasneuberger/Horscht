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

## Deployment

### Prerequisites
- Azure subscription
- Azure CLI installed and logged in
- Appropriate permissions to create resources

### Using PowerShell Script

```powershell
.\Deploy-AzureResourceGroup.ps1 `
    -ResourceGroupName "rg-horscht-dev" `
    -TemplateFile "main.bicep" `
    -TemplateParametersFile "main.parameters.json"
```

### Using Azure CLI

```bash
az deployment sub create \
    --location westeurope \
    --template-file main.bicep \
    --parameters environment=dev
```

## Local Development

For local development, use .NET Aspire instead of deploying to Azure:
1. Run the `Horscht.AppHost` project
2. Azurite (local Azure Storage emulator) will be started automatically
3. All storage resources (queue, table) are created automatically on startup

See [ASPIRE.md](../ASPIRE.md) for more information about local development with Aspire.
