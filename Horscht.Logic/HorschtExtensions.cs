using Horscht.Logic.Options;
using Horscht.Logic.Services;
using Microsoft.Extensions.Configuration;

namespace Horscht.Logic;

public static class HorschtExtensions
{
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<StorageOptions>()
            .Bind(configuration.GetSection("Storage"))
            .ValidateDataAnnotations();

        return services;
    }

    public static IServiceCollection AddUpload(this IServiceCollection services)
    {
        services.AddScoped<IUploadService, UploadService>();

        return services;
    }
}
