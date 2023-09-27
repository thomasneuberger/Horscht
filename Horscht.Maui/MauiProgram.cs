using Horscht.App.Services;
using Horscht.Maui.Authentication;
using Horscht.Maui.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Horscht.Maui;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        builder.Configuration
            .AddJsonFile("appsettings.json");

        builder.Services.AddOptions<AuthenticationOptions>()
            .Bind(builder.Configuration.GetSection("AzureAd"))
            .ValidateDataAnnotations();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton(SecureStorage.Default);
        builder.Services.AddAuthorizationCore();
        builder.Services.TryAddScoped<AuthenticationStateProvider, AuthenticationStateService>();

        builder.Services.AddSingleton<WeatherForecastService>();
        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

        return builder.Build();
    }
}
