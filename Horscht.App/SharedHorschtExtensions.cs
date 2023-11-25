using Horscht.Logic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Horscht.App.Services;
using Horscht.Contracts.Services;

namespace Horscht.App;

public static class SharedHorschtExtensions
{
    public static IServiceCollection AddSharedHorschtServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddJsonSerializerDefaults();

        services.AddScoped<IStorageClientProvider, StorageClientProvider>();
        services.AddStorage(configuration);

        services.AddUpload();

        return services;
    }
}
