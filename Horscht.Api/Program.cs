using Horscht.Api.Services;
using Horscht.Contracts.Options;
using Horscht.Contracts.Services;
using Horscht.Logic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

namespace Horscht.Api;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add service defaults & Aspire client integrations.
        builder.AddServiceDefaults();

        // Add Aspire Azure Storage clients
        builder.AddAzureBlobServiceClient("blobs");
        builder.AddAzureQueueServiceClient("queues");
        builder.AddAzureTableServiceClient("tables");

        // Add services to the container.
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        builder.Services.AddJsonSerializerDefaults();

        builder.Services.AddSingleton<IStorageClientProvider, StorageClientProvider>();

        builder.Services.AddOptions<AppStorageOptions>()
            .Bind(builder.Configuration.GetSection("Storage"))
            .ValidateDataAnnotations();

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
        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.ProvideSwagger(builder.Configuration);
        }

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
