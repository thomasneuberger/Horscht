using Horscht.App.Services;
using Microsoft.Identity.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Horscht.Maui.Authentication;
public class AuthenticationService : IAuthenticationService
{
    private readonly IPublicClientApplication _authenticationClient;

    public AuthenticationService()
    {
        _authenticationClient = PublicClientApplicationBuilder.Create(Constants.ClientId)
            .WithTenantId(Constants.TenantId)
            //.WithB2CAuthority(Constants.AuthoritySignIn) // uncomment to support B2C
#if WINDOWS
            .WithRedirectUri("http://localhost")
#else
            .WithRedirectUri($"msal{Constants.ClientId}://auth")
#endif
            .Build();
    }

    public ClaimsIdentity? CurrentUser { get; private set; }
    public event EventHandler<ClaimsIdentity?>? CurrentUserChanged;

    public async Task LoginAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authenticationClient
                .AcquireTokenInteractive(Constants.Scopes)
                .WithPrompt(Prompt.ForceLogin)
#if ANDROID
                .WithParentActivityOrWindow(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity)
#endif
#if WINDOWS
                .WithUseEmbeddedWebView(false)
#endif
                .ExecuteAsync(cancellationToken);

            var idToken = result?.IdToken; // you can also get AccessToken if you need it
            if (idToken != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(idToken);
                if (jwtToken is not null)
                {
                    var claims = jwtToken.Claims.ToList();
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine($"Name: {claims.FirstOrDefault(x => x.Type.Equals("name"))?.Value}");
                    stringBuilder.AppendLine($"Email: {claims.FirstOrDefault(x => x.Type.Equals("preferred_username"))?.Value}");
                    Console.WriteLine(stringBuilder.ToString());
                    //LoginResultLabel.Text = stringBuilder.ToString();

                    CurrentUser = new ClaimsIdentity(claims, _authenticationClient.GetType().Name);
                }
            }
            else
            {
                CurrentUser = null;
            }
        }
        catch (MsalClientException)
        {
            CurrentUser = null;
        }

        NotifyCurrentUserChanged();
    }

    public Task LogoutAsync(CancellationToken cancellationToken)
    {
        CurrentUser = null;

        NotifyCurrentUserChanged();

        return Task.CompletedTask;
    }

    private void NotifyCurrentUserChanged()
    {
        CurrentUserChanged?.Invoke(this, CurrentUser);
    }
}
