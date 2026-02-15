using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Horscht.Api;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services, IConfiguration configuration)
    {
        var tenantId = configuration["AzureAd:TenantId"];
        var clientId = configuration["AzureAd:ClientId"];
        var scope = $"api://{clientId}/access_as_user";

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            // Swashbuckle 10.x uses a delegate pattern for AddSecurityRequirement
            // The document parameter is used by OpenApiSecuritySchemeReference to properly link the requirement to the definition
            // Include the scope in the list to preselect it in Swagger UI
            c.AddSecurityRequirement(document =>
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("oauth2", document)] = new List<string> { scope }
                });

            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    // Use AuthorizationCode flow instead of deprecated Implicit flow
                    // This fixes the "response_type 'token' is not enabled" error
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                        TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { scope, "Access the API" }
                        }
                    }
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder ProvideSwagger(this IApplicationBuilder app, IConfiguration configuration)
    {
        var clientId = configuration["AzureAd:ClientId"];
        var clientSecret = configuration["AzureAd:ClientSecret"];
        var scope = $"api://{clientId}/access_as_user";

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.OAuthAppName("Swagger");
            options.OAuthClientId(clientId);
            options.OAuthClientSecret(clientSecret);
            options.OAuthUseBasicAuthenticationWithAccessCodeGrant();
            // Enable PKCE (Proof Key for Code Exchange) for secure browser-based OAuth
            // This is required for SPAs and fixes "Cross-origin token redemption" error
            options.OAuthUsePkce();
            // Preselect the scope so users don't have to manually check it
            options.OAuthScopes(scope);
        });

        return app;
    }
}
