using Horscht.App;
using Horscht.App.Shared;
using Horscht.Contracts;
using Horscht.Contracts.Services;
using Horscht.Web.Authentication;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Horscht.Web.Client;
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Configuration.AddUserSecrets<Program>();

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

        builder.Services.AddSharedHorschtServices(builder.Configuration);

        builder.Services.AddMsalAuthentication(options =>
        {
            builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
            options.ProviderOptions.DefaultAccessTokenScopes.Add(StorageConstants.Scope);
            //options.ProviderOptions.AdditionalScopesToConsent = new List<string>
            //{
            //    StorageConstants.Scope
            //};
        });

        await builder.Build().RunAsync();
    }
}
