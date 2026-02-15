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

To support Swagger UI OAuth authentication in development, you need to configure the app registration:

1. Go to **Authentication**
2. Click **Add a platform** → **Single-page application**
3. Add the Swagger redirect URI:
   - For local development: `https://localhost:7100/swagger/oauth2-redirect.html` (adjust port to match your API's port)
   - You may need to add multiple URIs for different ports (e.g., 5100, 7100, etc.)
   - Click **Configure**
4. Under **Implicit grant and hybrid flows**, ensure nothing is checked (API uses Authorization Code with PKCE)

**Why Single-Page Application platform:**
- Swagger UI is a browser-based JavaScript application (like a SPA)
- It uses Authorization Code flow with PKCE (Proof Key for Code Exchange)
- PKCE is the modern, secure way for browser-based apps to authenticate
- The SPA platform type is designed for this exact scenario

**Note**: With PKCE enabled, you don't need to set "Allow public client flows" to Yes. The SPA platform handles this correctly.

#### Create a Client Secret

#### Create a Client Secret

Client secrets are required for the API to authenticate with Azure AD. The primary use cases are:
- **Service-to-service authentication**: Allows the API to authenticate itself when calling other Azure services
- **Server-side OAuth flows**: Used in confidential client scenarios (not needed for Swagger UI with PKCE)

**Note for Swagger UI**: When using the SPA platform with PKCE (our current setup), Swagger UI does NOT use the client secret. PKCE provides security without requiring a secret, which is appropriate for browser-based applications.

1. In the app registration, go to **Certificates & secrets**
2. Click **New client secret**
3. Configure:
   - **Description**: Give it a meaningful name (e.g., "Development Secret" or "Production Secret")
   - **Expires**: Choose an expiration period (recommended: 6 months or 1 year for production, 90 days for development)
4. Click **Add**
5. **IMPORTANT**: Copy the **Value** immediately - this is your client secret and it will only be shown once
   - Store it securely (e.g., in Azure Key Vault for production, or User Secrets for local development)
   - Never commit this value to source control
6. Note the **Secret ID** for reference (this is not the secret itself, just an identifier)

⚠️ **Security Warning**: Client secrets are sensitive credentials. If a secret is compromised:
- Immediately delete it from the Azure Portal (Certificates & secrets → Delete)
- Create a new secret
- Update all applications using the old secret

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

#### Create the App Registration (if needed)

1. Go to [Azure Portal](https://portal.azure.com) → Azure Active Directory → App registrations
2. Click **New registration**
3. Configure:
   - **Name**: `Horscht.Importer` (or `Horscht.Importer-dev`, `Horscht.Importer-prod`)
   - **Supported account types**: Accounts in this organizational directory only (Single tenant)
   - **Redirect URI**: Leave empty initially (will add Swagger redirect URI in next step)
4. Click **Register**
5. Note the **Application (client) ID**

#### Configure Authentication (for Swagger UI support)

To enable Swagger UI OAuth authentication for the Importer service in development:

1. Go to **Authentication**
2. Click **Add a platform** → **Single-page application**
3. Add the Swagger redirect URI:
   - For local development: `https://localhost:7140/swagger/oauth2-redirect.html` (adjust port to match your Importer's port)
   - Common ports: 5140, 7140
   - Click **Configure**
4. Under **Implicit grant and hybrid flows**, ensure nothing is checked (uses Authorization Code with PKCE)

**Note**: The Importer uses the SPA platform type for Swagger UI, which supports PKCE authentication. This is the secure, modern approach for browser-based OAuth flows.

#### Create a Client Secret

The Importer service needs a client secret to authenticate with Azure AD.

1. In the app registration, go to **Certificates & secrets**
2. Click **New client secret**
3. Configure:
   - **Description**: Give it a meaningful name (e.g., "Importer Development Secret")
   - **Expires**: Choose an expiration period (recommended: 6 months or 1 year for production)
4. Click **Add**
5. **IMPORTANT**: Copy the **Value** immediately and store it securely
   - This value is shown only once
   - For local development, use .NET User Secrets (see Local Development section below)
   - For production, store in Azure Key Vault

⚠️ **Note**: The Importer uses the client secret to authenticate as itself (client credentials flow) when accessing Azure resources.

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
    "ClientSecret": "your-api-client-secret",
    "Scopes": "access_as_user"
  }
}
```

**Where to get these values:**
- `TenantId`: From Azure Portal → Azure Active Directory → Overview → Tenant ID
- `ClientId`: From your API app registration → Overview → Application (client) ID
- `ClientSecret`: From your API app registration → Certificates & secrets (created in the setup steps above)
- `Domain`: Your Azure AD domain (usually `yourcompany.onmicrosoft.com`)

⚠️ **Important**: Never commit the `ClientSecret` to source control. For local development, use .NET User Secrets (see Local Development section). For production deployment via Bicep templates, these values are configured in the `deployment/main.bicep` parameters.

### Horscht.Importer Configuration

Update the `appsettings.json` for the Importer service:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "your-tenant.onmicrosoft.com",
    "TenantId": "your-tenant-id",
    "ClientId": "your-importer-client-id",
    "ClientSecret": "your-importer-client-secret",
    "Scopes": "access_as_user"
  }
}
```

**Where to get these values:**
- `TenantId`: From Azure Portal → Azure Active Directory → Overview → Tenant ID
- `ClientId`: From your Importer app registration → Overview → Application (client) ID
- `ClientSecret`: From your Importer app registration → Certificates & secrets (created in the setup steps above)
- `Domain`: Your Azure AD domain

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

### Setting up User Secrets

To avoid committing sensitive information like client secrets to source control, use .NET User Secrets for local development:

1. **For Horscht.Api**, navigate to the project directory and run:
   ```bash
   cd Horscht.Api
   dotnet user-secrets init
   dotnet user-secrets set "AzureAd:ClientSecret" "your-api-client-secret"
   ```

2. **For Horscht.Importer**, navigate to the project directory and run:
   ```bash
   cd Horscht.Importer
   dotnet user-secrets init
   dotnet user-secrets set "AzureAd:ClientSecret" "your-importer-client-secret"
   ```

These secrets are stored securely on your local machine and won't be committed to source control.

### Configuration

1. Create a development app registration for the API with redirect URI `https://localhost:5100` (or your local port)
2. Create client secrets for both API and Importer app registrations (see sections above)
3. Store the client secrets using User Secrets (see above)
4. Update `Horscht.Api/appsettings.Development.json` with your development API client ID
5. Update `Horscht.Importer/appsettings.Development.json` with your development Importer client ID
6. Update `Horscht.Web/wwwroot/appsettings.Development.json`:
   - Set `AzureAd.ClientId` to your Web app's development client ID
   - Set `Api.BaseUrl` to `https://localhost:5100` (or the port where API runs locally)
   - Set `Api.Scopes` to your API's scope URI

7. When running via AppHost, the services will discover each other automatically, but you still need valid Azure AD configuration for authentication to work.

## Testing with Swagger UI

The API and Importer services include Swagger UI for interactive API testing in Development mode. The client secret you configured enables OAuth 2.0 authentication in Swagger.

### Prerequisites for Swagger Authentication

Before using Swagger UI authentication, ensure your Azure AD app registration is configured correctly:

✅ **Required Configuration:**
1. **Redirect URI added**: `https://localhost:{port}/swagger/oauth2-redirect.html`
   - For API: Usually port 7100 or 5100
   - For Importer: Usually port 7140 or 5140
   - Add this in: Azure Portal → App Registration → Authentication → Add platform (**Single-page application**)

2. **Platform type**: Single-page application (SPA)
   - Required because Swagger UI is a browser-based JavaScript application
   - Supports PKCE (Proof Key for Code Exchange) for secure authentication
   - No need to enable "Allow public client flows" when using SPA platform

3. **Client secret**: NOT required for Swagger UI
   - Swagger uses PKCE instead of client secrets (more secure for browser-based apps)
   - Client secrets can be used for other purposes (service-to-service auth)
   - But they are not needed for Swagger authentication

If these are not configured, you'll see errors like:
- "No reply address is registered for the application"
- "Cross-origin token redemption is permitted only for the 'Single-Page Application' client-type"
- "Client is public so neither 'client_assertion' nor 'client_secret' should be presented"

### How Client Secret Enables Swagger Authentication

When you configure the `AzureAd:ClientSecret` in your application settings (or User Secrets), the Swagger UI uses it to authenticate with Azure AD on your behalf. This allows you to:

1. Authenticate with your Azure AD account through Swagger UI
2. Obtain access tokens automatically
3. Test secured API endpoints without manually managing tokens

### Using Swagger UI with Authentication

1. **Start the API** (in Development mode):
   ```bash
   cd Horscht.Api
   dotnet run
   ```

2. **Open Swagger UI** in your browser:
   - Navigate to `https://localhost:{port}/swagger` (e.g., `https://localhost:7100/swagger`)
   - You'll see the Swagger UI interface with all available endpoints

3. **Authenticate**:
   - Click the **"Authorize"** button (lock icon) at the top right of the Swagger UI
   - In the authorization dialog, you'll see the OAuth2 (oauth2) section
   - The `client_id` is pre-filled from your configuration
   - Click **"Authorize"** button
   - You'll be redirected to the Microsoft login page
   - Sign in with your Azure AD credentials
   - After successful login, you'll be redirected back to Swagger UI
   - You should see "Authorized" with your scopes listed

4. **Test Endpoints**:
   - Once authorized, all API requests from Swagger will include your access token
   - Click on any endpoint to expand it
   - Click **"Try it out"**
   - Fill in any required parameters
   - Click **"Execute"**
   - The request will include the `Authorization: Bearer {token}` header automatically

### What Happens Behind the Scenes

When you click "Authorize" in Swagger:

1. Swagger generates a PKCE code verifier and challenge (random strings for security)
2. Swagger initiates an OAuth2 authorization code flow with PKCE to Azure AD
3. It includes the `ClientId` and PKCE code challenge in the authorization request (NO client secret)
4. Azure AD authenticates you and asks for consent (if needed)
5. Azure AD returns an authorization code
6. Swagger exchanges the code for an access token using ONLY the PKCE code verifier
7. The PKCE verification ensures the token exchange is from the same client that initiated the flow
8. The access token is stored and used for all subsequent API requests

**About PKCE**: Proof Key for Code Exchange is a security extension that prevents authorization code interception attacks. It's the recommended approach for all OAuth flows in browser-based applications. With PKCE, no client secret is needed - the dynamic code verifier/challenge pair provides the security.

### Troubleshooting Swagger Authentication

**"No reply address is registered for the application" error:**
- The Swagger OAuth redirect URI is not configured in your Azure AD app registration
- Solution:
  1. Go to your app registration in Azure Portal → Authentication
  2. Add platform: **Web**
  3. Add redirect URI: `https://localhost:{port}/swagger/oauth2-redirect.html`
  4. Replace `{port}` with your actual port (e.g., 7100 for API, 7140 for Importer)
  5. You can add multiple redirect URIs for different ports
- See the "Configure Authentication" section in the app registration setup above

**"Client is public so neither 'client_assertion' nor 'client_secret' should be presented" error:**
- The app registration is configured as SPA (public client), but the code is still sending a client secret
- Public clients (like SPAs) cannot use client secrets - they use PKCE for security instead
- Solution: Ensure you're using the latest code which does NOT send client secret for Swagger
- The fix: Remove `options.OAuthClientSecret(clientSecret)` from SwaggerExtensions.cs
- This is already fixed in the latest version - with PKCE, no client secret is needed or allowed

**"Cross-origin token redemption is permitted only for the 'Single-Page Application' client-type" error:**
- The app registration is not configured as a Single-Page Application (SPA)
- This error occurs when using Authorization Code flow from a browser without PKCE or SPA platform
- Solution:
  1. Go to your app registration in Azure Portal → Authentication
  2. If you have a "Web" platform with the Swagger redirect URI, remove it
  3. Click **Add a platform** → **Single-page application**
  4. Add redirect URI: `https://localhost:{port}/swagger/oauth2-redirect.html`
  5. Click **Configure**
  6. Ensure the code uses `options.OAuthUsePkce()` (already configured in latest code)
- Why this fixes it: SPA platform + PKCE is designed for browser-based OAuth flows
- No need to enable "Allow public client flows" when using SPA platform

**"response_type 'token' is not enabled for the application" error:**
- This error occurred with the old Implicit flow configuration (now fixed)
- The application now uses Authorization Code flow which is the recommended OAuth2 flow
- If you still see this error, ensure you're using the latest version of the code

**Scope not preselected in authorization dialog:**
- This has been fixed - the scope is now automatically preselected
- If you still need to manually select it, ensure your configuration is up to date

**"Failed to fetch" or CORS errors:**
- Ensure you're running in Development mode (Swagger is only enabled in Development)
- Check that your redirect URI is configured in the app registration
- For Swagger, the redirect URI should be: `https://localhost:{port}/swagger/oauth2-redirect.html`

**"Unauthorized" (401) responses even after authorizing:**
- Verify the API's client ID in configuration matches the app registration
- Check that the scope in SwaggerExtensions.cs matches the exposed API scope
- Ensure the access token has the correct audience claim (should be your API's client ID)

**Cannot sign in:**
- Verify `TenantId` is correct in appsettings
- Check that your user account exists in the Azure AD tenant
- Ensure the app registration allows users from your tenant

### Security Note

The client secret used by Swagger is only for development and testing purposes. In production:
- Swagger UI should be disabled (it's automatically disabled when not in Development mode)
- Client secrets should be stored in Azure Key Vault
- Consider using certificate-based authentication instead of secrets for production services

## Production Deployment

When deploying to Azure Container Apps:

### Storing Secrets Securely

1. **Create an Azure Key Vault** (if you don't have one):
   ```bash
   az keyvault create --name "kv-horscht-prod" --resource-group "rg-horscht-prod" --location "westeurope"
   ```

2. **Store your client secrets in Key Vault**:
   ```bash
   # Store API client secret
   az keyvault secret set --vault-name "kv-horscht-prod" --name "api-client-secret" --value "your-api-client-secret"
   
   # Store Importer client secret
   az keyvault secret set --vault-name "kv-horscht-prod" --name "importer-client-secret" --value "your-importer-client-secret"
   ```

3. **Configure Bicep Templates**:
   - Update `deployment/main.bicep` with your production app registration IDs
   - Reference Key Vault secrets for the `authClientSecret` parameters
   - The Container App will use managed identity where possible, but the client ID and tenant ID are still needed for JWT validation

### Deployment Parameters

When deploying, provide the following parameters:
- `authClientId`: The application (client) ID from the app registration
- `authClientSecret`: Retrieved from Key Vault (never hardcode this)
- `tenantId`: Your Azure AD tenant ID

Example deployment command:
```bash
az deployment sub create \
  --location westeurope \
  --template-file deployment/main.bicep \
  --parameters \
    authClientId="your-client-id" \
    authClientSecret="@Microsoft.KeyVault(SecretUri=https://kv-horscht-prod.vault.azure.net/secrets/api-client-secret/)" \
    tenantId="your-tenant-id"
```

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
