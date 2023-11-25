using Horscht.Contracts.Options;
using Horscht.Contracts.Services;
using Horscht.Importer.HostedServices;
using Horscht.Importer.Services;
using Horscht.Logic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

namespace Horscht.Importer;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        builder.Services.AddJsonSerializerDefaults();

        builder.Services.AddSingleton<IStorageClientProvider, StorageClientProvider>();

        builder.Services.AddHostedService<FileImport>();

        builder.Services.AddOptions<ImporterStorageOptions>()
            .Bind(builder.Configuration.GetSection("Storage"))
            .ValidateDataAnnotations();
        builder.Services.AddOptions<AppStorageOptions>()
            .Bind(builder.Configuration.GetSection("Storage"))
            .ValidateDataAnnotations();

        builder.Services.AddImport();

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        builder.Services.AddControllers();

        builder.Services.AddSwaggerServices(builder.Configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.ProvideSwagger(builder.Configuration);
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
