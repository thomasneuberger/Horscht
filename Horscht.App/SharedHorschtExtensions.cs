using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horscht.Logic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Horscht.App;

public static class SharedHorschtExtensions
{
    public static IServiceCollection AddSharedHorschtServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStorage(configuration);

        services.AddUpload();

        return services;
    }
}
