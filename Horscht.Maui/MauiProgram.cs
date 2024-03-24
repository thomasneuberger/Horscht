using Horscht.App;
using Horscht.App.Shared;
using Horscht.Contracts.Services;
using Horscht.Maui.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
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
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<MainLayout>();

        builder.Services.AddOptions<AuthenticationOptions>()
            .Bind(builder.Configuration.GetSection("AzureAd"))
            .ValidateDataAnnotations();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton(SecureStorage.Default);
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<AuthenticationStateProvider, AuthenticationStateService>();
        //builder.Services.AddScoped<IAccessTokenProvider, AccessTokenProvider>();

        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

        builder.Services.AddSharedHorschtServices(builder.Configuration);

        return builder.Build();
    }
}
