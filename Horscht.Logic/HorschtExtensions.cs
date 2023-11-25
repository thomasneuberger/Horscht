using Horscht.Contracts.Options;
using Horscht.Contracts.Services;
using Horscht.Logic.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Horscht.Logic;

public static class HorschtExtensions
{
    public static IServiceCollection AddJsonSerializerDefaults(this IServiceCollection services)
    {
        services.AddSingleton(new JsonSerializerOptions(JsonSerializerOptions.Default)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        return services;
    }

    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AppStorageOptions>()
            .Bind(configuration.GetSection("Storage"))
            .ValidateDataAnnotations();

        return services;
    }

    public static IServiceCollection AddUpload(this IServiceCollection services)
    {
        services.AddScoped<IUploadService, UploadService>();

        return services;
    }

    public static IServiceCollection AddImport(this IServiceCollection services)
    {
        services.AddSingleton<IImportService, ImportService>();

        return services;
    }
}
