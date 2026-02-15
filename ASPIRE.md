# .NET Aspire Integration

This document describes the .NET Aspire integration for the Horscht music catalog application.

## Overview

The Horscht application now includes .NET Aspire for local development orchestration. Aspire provides:

- **Service orchestration** - Run all services together from a single entry point
- **Local Azure Storage emulation** - Uses Azurite containers instead of real Azure resources
- **Service discovery** - Automatic configuration of service endpoints
- **Telemetry** - OpenTelemetry integration for logs, metrics, and traces
- **Health checks** - Built-in health monitoring for all services
- **Resilience patterns** - Automatic retry and circuit breaker patterns

## Projects

### Horscht.AppHost

The AppHost is the orchestration entry point for the application. It defines:

- **Azure Storage Emulator (Azurite)** - Docker container providing local Blob, Queue, and Table storage
- **Horscht.Importer** - The background import service
- **Horscht.Web** - The Blazor WebAssembly frontend

### Horscht.ServiceDefaults

The ServiceDefaults project provides common configuration used by all services:

- OpenTelemetry configuration (logs, metrics, traces)
- Health check endpoints
- Service discovery
- HTTP client resilience (retry, circuit breaker, timeout)

## Running Locally

### Prerequisites

1. Visual Studio 2022 (17.8+) or Rider with .NET Aspire workload installed
2. Docker Desktop running
3. .NET 10 SDK

### Starting the Application

#### From Visual Studio
1. Set `Horscht.AppHost` as the startup project
2. Press F5 to run
3. The Aspire Dashboard will open automatically showing all services

#### From Command Line
```bash
cd Horscht.AppHost
dotnet run
```

The Aspire Dashboard will be available at the URL shown in the console output (typically https://localhost:17145).

## Architecture Changes

### Horscht.Importer

The Importer service now uses Aspire Azure Storage client integration:

- `BlobServiceClient` - Injected via DI, configured to connect to Azurite
- `QueueServiceClient` - Injected via DI, configured to connect to Azurite  
- `TableServiceClient` - Injected via DI, configured to connect to Azurite

The `StorageClientProvider` has been updated to use these injected clients instead of creating clients from connection strings.

### Horscht.Web

The Web project (Blazor WebAssembly) has been updated to support both local development with Azurite and production deployment with Azure Storage:

- **Local Development**: Uses `appsettings.Development.json` which includes the Azurite connection string for anonymous access to the local emulator
- **Production**: Uses `appsettings.json` with Azure AD authentication for secure access to real Azure Storage

The `StorageClientProvider` in Horscht.App automatically detects whether a connection string is available:
- If `ConnectionString` is configured (local development), it uses connection string authentication
- If not (production), it uses Azure AD token-based authentication

### CORS Configuration

Since Blazor WebAssembly runs in the browser, it needs to make cross-origin requests to Azurite. The AppHost configures Azurite with CORS enabled for all origins during local development:

```csharp
.RunAsEmulator(emulator =>
{
    emulator.WithBlobPort(10000)
            .WithQueuePort(10001)
            .WithTablePort(10002)
            .WithArgs(
                "--blobCors", "*",
                "--queueCors", "*", 
                "--tableCors", "*");
});
```

**Important**: The wildcard (`*`) CORS configuration is only safe for local development. Production Azure Storage should use specific allowed origins configured in the Azure Portal.

## Configuration

### Local Development (Azurite)

When running via Aspire, the Blazor WebAssembly app uses configuration from `wwwroot/appsettings.Development.json`:

```json
{
  "Storage": {
    "ConnectionString": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;..."
  }
}
```

This connection string is the well-known Azurite development storage account, which:
- Only works locally against the Azurite emulator
- Is publicly documented and safe for local development
- Allows the browser-based app to connect without Azure AD authentication

### Production Deployment

Production uses `appsettings.json` without a connection string, triggering Azure AD authentication:

```json
{
  "Storage": {
    "BlobUri": "https://youraccountname.blob.core.windows.net/",
    "TableUri": "https://youraccountname.table.core.windows.net/",
    ...
  }
}
```

Aspire automatically configures connection strings for server-side services (Horscht.Importer). The connection names defined in the AppHost (`blobs`, `queues`, `tables`) are automatically mapped to the client registrations in the services.

For production deployments, you can switch from Azurite to real Azure Storage by:
1. Removing the `.RunAsEmulator()` configuration in AppHost
2. Providing Azure Storage connection strings via configuration

## Benefits

- **Faster development** - No need to provision real Azure resources for local development
- **Better debugging** - See all services and their logs in one dashboard
- **Easier onboarding** - New developers can run the entire stack with one command
- **Production-like** - Uses the same Azure SDK clients as production
- **Free** - No Azure costs for local development

## Troubleshooting

### Docker container not starting
- Ensure Docker Desktop is running
- Check that ports 10000, 10001, 10002 are not in use

### Services not connecting to Azurite
- Check the Aspire Dashboard for service status
- Verify that the Azurite container is running and healthy
- Check service logs in the Aspire Dashboard

### CORS errors when accessing storage from browser
If you see errors like "No 'Access-Control-Allow-Origin' header is present":
- Verify that Azurite is configured with CORS in `AppHost.cs` (see CORS Configuration section)
- Check that the Azurite container started with the CORS arguments
- Ensure you're accessing Azurite via the correct URLs (127.0.0.1 with ports 10000/10001/10002)
- Clear browser cache and retry

### Aspire Dashboard not opening
- Check that the dashboard ports (17145, 15182) are not in use
- Try running from Visual Studio instead of command line

## Learn More

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Aspire Azure Storage Integration](https://learn.microsoft.com/en-us/dotnet/aspire/storage/azure-storage-integrations)
- [Azurite - Azure Storage Emulator](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite)
