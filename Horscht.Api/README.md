# Horscht.Api

REST API serving as the Backend for Frontend (BFF) for the Horscht music catalog application.

## Overview

This API provides endpoints for the Blazor WebAssembly frontend to interact with the backend services and Azure Storage. It handles authentication, authorization, and business logic.

## Authentication

**IMPORTANT**: This API requires its own Azure AD app registration.

### Setup Steps

1. **Create Azure AD App Registration** for the API
   - See [AUTHENTICATION.md](../AUTHENTICATION.md) for detailed instructions
   - The API needs to expose the `access_as_user` scope
   - Note the Client ID for configuration

2. **Configure the Web App** to call this API
   - Add API permissions in the Web app registration
   - Request the `api://{api-client-id}/access_as_user` scope
   - See [AUTHENTICATION.md](../AUTHENTICATION.md) for complete setup

3. **Update Configuration**
   - Set the API's Client ID in `appsettings.json` → `AzureAd:ClientId`
   - For local development, update `appsettings.Development.json`
   - For production, configure via Bicep parameters in `deployment/api.bicep`

## Configuration

### Required Settings

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "your-tenant.onmicrosoft.com",
    "TenantId": "your-tenant-id",
    "ClientId": "your-api-client-id",  // ← This must be the API's own Client ID
    "Scopes": "access_as_user"
  },
  "Storage": {
    "ConnectionString": "your-storage-connection-string",
    "FileContainerName": "files",
    "AlbumContainerName": "albums",
    "UploadQueueName": "upload",
    "ImportQueueName": "import"
  }
}
```

## Running Locally

### Via .NET Aspire (Recommended)

1. Ensure Docker Desktop is running
2. Set `Horscht.AppHost` as the startup project
3. Press F5
4. The API will start automatically along with other services
5. Access Swagger UI at: `https://localhost:{port}/swagger`

### Standalone

```bash
dotnet run --project Horscht.Api
```

Access Swagger UI at: `https://localhost:7100/swagger` (or configured port)

## Development Features

### Swagger UI

When running in Development mode, Swagger UI is automatically enabled at `/swagger`. This provides:
- Interactive API documentation
- Ability to test endpoints directly from the browser
- OAuth 2.0 authentication for testing secured endpoints

### Health Checks

Health endpoint available at `/api/health` for container probes and monitoring.

## Project Structure

- **Program.cs** - Application startup and service configuration
- **SwaggerExtensions.cs** - Swagger/OpenAPI configuration with Azure AD auth
- **Services/** - Service implementations (e.g., StorageClientProvider)
- **Controllers/** - API endpoint controllers (add your controllers here)

## Adding Endpoints

To add new API endpoints:

1. Create a controller in the `Controllers/` directory
2. Inherit from `ControllerBase`
3. Add the `[ApiController]` and `[Route]` attributes
4. Controllers automatically require authentication (fallback policy)

Example:

```csharp
[ApiController]
[Route("api/[controller]")]
public class SongsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSongs()
    {
        // Your implementation
    }
}
```

## Authorization

The API uses a fallback authorization policy that requires all users to be authenticated. To allow anonymous access to specific endpoints:

```csharp
[AllowAnonymous]
[HttpGet("public")]
public IActionResult PublicEndpoint()
{
    // Accessible without authentication
}
```

## Deployment

The API is deployed as an Azure Container App via Bicep templates.

### Scale to Zero

The API is configured to scale to zero when not in use:
- **minReplicas**: 0
- **maxReplicas**: 10
- **Scaling rule**: HTTP requests (100 concurrent requests threshold)

### Required Parameters

When deploying via `deployment/main.bicep`, provide:
- `authClientId` - The API's Azure AD Client ID
- `authClientSecret` - The API's Azure AD Client Secret (store in Key Vault for production)
- `apiHostname` - The custom domain for the API
- `imageTag` - The container image tag to deploy

See `deployment/api.bicep` for all configuration options.

## Security Considerations

1. **Never commit secrets** - Use user secrets locally, Key Vault in production
2. **Validate tokens** - The API automatically validates JWT tokens from Azure AD
3. **Use HTTPS** - Always use HTTPS in production
4. **Separate app registrations** - Don't share Client IDs between Web and API
5. **Principle of least privilege** - Only expose necessary scopes and permissions

## References

- [AUTHENTICATION.md](../AUTHENTICATION.md) - Complete authentication setup guide
- [ARCHITECTURE.md](../ARCHITECTURE.md) - Overall application architecture
- [Microsoft Identity Platform](https://docs.microsoft.com/en-us/azure/active-directory/develop/)
