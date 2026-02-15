# Azure AD Authentication Setup

This document describes how to set up Azure AD app registrations for the Horscht application.

## Overview

Horscht uses three separate Azure AD app registrations:

1. **Horscht.Web** - Frontend Blazor WebAssembly application
2. **Horscht.Api** - Backend for Frontend (BFF) REST API
3. **Horscht.Importer** - Background processing service

This separation follows security best practices where each component has its own identity and permissions.

## App Registration Setup

### 1. Horscht.Api App Registration

The API needs its own app registration to validate tokens from the Web application.

#### Create the App Registration

1. Go to [Azure Portal](https://portal.azure.com) → Azure Active Directory → App registrations
2. Click **New registration**
3. Configure:
   - **Name**: `Horscht.Api` (or `Horscht.Api-dev`, `Horscht.Api-prod` for different environments)
   - **Supported account types**: Accounts in this organizational directory only (Single tenant)
   - **Redirect URI**: Leave empty (API doesn't need redirect URIs)
4. Click **Register**
5. Note the **Application (client) ID** - you'll need this later

#### Expose an API

1. In the app registration, go to **Expose an API**
2. Click **Add a scope**
3. For Application ID URI, accept the default `api://{clientId}` or customize it
4. Click **Save and continue**
5. Add a scope:
   - **Scope name**: `access_as_user`
   - **Who can consent**: Admins and users
   - **Admin consent display name**: Access Horscht API
   - **Admin consent description**: Allows the application to access the Horscht API on behalf of the signed-in user
   - **User consent display name**: Access your Horscht data
   - **User consent description**: Allows the application to access your Horscht data on your behalf
   - **State**: Enabled
6. Click **Add scope**

#### Configure Authentication

1. Go to **Authentication**
2. Under **Implicit grant and hybrid flows**, ensure nothing is checked (API uses bearer tokens)
3. Under **Allow public client flows**, set to **No**

#### Optional: Add App Roles (for future authorization)

1. Go to **App roles**
2. Click **Create app role**
3. Define roles as needed (e.g., `Admin`, `User`)

### 2. Horscht.Web App Registration

The Web application needs to be configured to call the API.

#### Create/Update the App Registration

If you don't already have a Web app registration:

1. Go to Azure Active Directory → App registrations
2. Click **New registration**
3. Configure:
   - **Name**: `Horscht.Web` (or `Horscht.Web-dev`, `Horscht.Web-prod`)
   - **Supported account types**: Accounts in this organizational directory only (Single tenant)
   - **Redirect URI**: 
     - Type: Single-page application (SPA)
     - URI: `https://localhost:7122/authentication/login-callback` (adjust port as needed)
     - For production: `https://your-domain.com/authentication/login-callback`
4. Click **Register**
5. Note the **Application (client) ID**

#### Configure API Permissions

1. In the Web app registration, go to **API permissions**
2. Click **Add a permission**
3. Select **My APIs** tab
4. Find and select **Horscht.Api**
5. Under **Delegated permissions**, check `access_as_user`
6. Click **Add permissions**
7. Click **Grant admin consent for [Your Organization]** (requires admin privileges)

#### Configure Authentication

1. Go to **Authentication**
2. Under **Implicit grant and hybrid flows**:
   - Check **Access tokens (used for implicit flows)**
   - Check **ID tokens (used for implicit and hybrid flows)**
3. For production, add additional redirect URIs as needed

### 3. Horscht.Importer App Registration

The Importer service has its own app registration (this may already exist).

Follow similar steps as the API, but the Importer typically doesn't need to expose scopes unless other services need to call it.

## Configuration

### Horscht.Api Configuration

Update the `appsettings.json` and environment-specific settings:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "your-tenant.onmicrosoft.com",
    "TenantId": "your-tenant-id",
    "ClientId": "your-api-client-id",
    "Scopes": "access_as_user"
  }
}
```

For production deployment via Bicep templates, these values are configured in the `deployment/main.bicep` parameters.

### Horscht.Web Configuration

Update `Horscht.Web/wwwroot/appsettings.json`:

```json
{
  "AzureAd": {
    "ClientId": "your-web-client-id",
    "Authority": "https://login.microsoftonline.com/your-tenant-id",
    "ValidateAuthority": true
  },
  "Api": {
    "BaseUrl": "https://localhost:5100",
    "Scopes": [
      "api://your-api-client-id/access_as_user"
    ]
  }
}
```

For production:
- `Api.BaseUrl` should point to your deployed API URL (e.g., `https://api-horscht-prod.azurecontainerapps.io`)
- Update scopes to match your API's Application ID URI

## Authentication Flow

### User Authentication Flow

1. **User accesses Horscht.Web**
   - User is redirected to Azure AD login
   - User authenticates with their credentials
   - Azure AD returns an ID token and access token for the Web app

2. **Web app calls Horscht.Api**
   - Web app requests an access token for the API scope (`api://your-api-client-id/access_as_user`)
   - Azure AD issues an access token with the API's client ID as the audience
   - Web app includes the access token in the `Authorization: Bearer <token>` header

3. **API validates the token**
   - API validates the token signature using Azure AD's public keys
   - API checks the token audience matches its client ID
   - API checks the token issuer is the expected tenant
   - If valid, the API processes the request

### Token Acquisition in Blazor WebAssembly

In your Blazor components, use the `IAccessTokenProvider`:

```csharp
@inject IAccessTokenProvider TokenProvider

private async Task CallApiAsync()
{
    var tokenResult = await TokenProvider.RequestAccessToken(
        new AccessTokenRequestOptions
        {
            Scopes = new[] { "api://your-api-client-id/access_as_user" }
        });

    if (tokenResult.TryGetToken(out var token))
    {
        // Use token.Value in Authorization header
        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token.Value);
            
        var response = await httpClient.GetAsync("https://api-url/endpoint");
        // Process response
    }
}
```

## Local Development

For local development with .NET Aspire:

1. Create a development app registration for the API with redirect URI `https://localhost:5100` (or your local port)
2. Update `Horscht.Api/appsettings.Development.json` with your development API client ID
3. Update `Horscht.Web/wwwroot/appsettings.Development.json`:
   - Set `AzureAd.ClientId` to your Web app's development client ID
   - Set `Api.BaseUrl` to `https://localhost:5100` (or the port where API runs locally)
   - Set `Api.Scopes` to your API's scope URI

4. When running via AppHost, the services will discover each other automatically, but you still need valid Azure AD configuration for authentication to work.

## Production Deployment

When deploying to Azure Container Apps:

1. Use Azure Key Vault to store secrets:
   - API Client Secret
   - Any other sensitive configuration

2. Update Bicep templates (`deployment/main.bicep` and `deployment/api.bicep`) with your production app registration IDs

3. The Container App will use managed identity where possible, but the client ID and tenant ID are still needed for JWT validation

## Troubleshooting

### Common Issues

**401 Unauthorized responses**
- Verify the access token includes the correct audience (should be the API's client ID or app ID URI)
- Check that the token issuer matches the expected tenant
- Ensure the scope is correctly configured in the Web app

**Token acquisition fails in Web app**
- Verify API permissions are granted in the Web app registration
- Ensure admin consent has been granted
- Check that the scope URI matches the API's exposed scope

**CORS errors**
- Ensure the API's CORS policy allows requests from the Web app's origin
- For local development, localhost origins must be explicitly allowed

### Debugging Tips

1. Decode tokens at [jwt.ms](https://jwt.ms) to inspect claims
2. Check the `aud` (audience) claim matches your API's client ID
3. Check the `scp` (scope) claim includes `access_as_user`
4. Verify the `iss` (issuer) claim matches your tenant

## Security Best Practices

1. **Never commit client secrets to source control** - Use user secrets for local development, Key Vault for production
2. **Use the minimum required scopes** - Don't request permissions your app doesn't need
3. **Validate tokens properly** - Always validate issuer, audience, and signature
4. **Use HTTPS everywhere** - Never send tokens over unencrypted connections
5. **Rotate secrets regularly** - Set up a process to rotate client secrets
6. **Monitor for suspicious activity** - Use Azure AD sign-in logs to detect anomalies

## References

- [Microsoft Identity Platform Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/)
- [Secure an ASP.NET Core Blazor WebAssembly app with Azure AD](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/azure-active-directory)
- [Protect a Web API with Azure AD](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-overview)
