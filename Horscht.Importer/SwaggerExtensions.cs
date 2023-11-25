using Microsoft.OpenApi.Models;

namespace Horscht.Importer;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services, IConfiguration configuration)
    {
        var tenantId = configuration["AzureAd:TenantId"];
        var clientId = configuration["AzureAd:ClientId"];

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        },
                        Scheme = "oauth2",
                        Name = "oauth2",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                        TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { $"api://{clientId}/access_as_user", "Access the API" }
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

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.OAuthAppName("Swagger");
            options.OAuthClientId(clientId);
            options.OAuthClientSecret(clientSecret);
            options.OAuthUseBasicAuthenticationWithAccessCodeGrant();
        });

        return app;
    }
}
