using Horscht.App.Services;
using Microsoft.Identity.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Horscht.Maui.Authentication;
public class AuthenticationService : IAuthenticationService
{
    private const string AccountIdentifierKey = "LoggedInAccount";
    private readonly IPublicClientApplication _authenticationClient;

    private readonly ISecureStorage _secureStorage;

    private MsalCacheHelper? _cacheHelper;

    public AuthenticationService(ISecureStorage secureStorage)
    {
        _secureStorage = secureStorage;

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

    private async Task EnsureRegisteredCache()
    {
        if (_cacheHelper is null)
        {
            var storageProperties = new StorageCreationPropertiesBuilder("LoginCache", FileSystem.CacheDirectory)
                .Build();

            _cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
            _cacheHelper.RegisterCache(_authenticationClient.UserTokenCache);
        }
    }

    public async Task TryLoginSilentAsync(CancellationToken cancellationToken)
    {
        await EnsureRegisteredCache();

        var identifier = await _secureStorage.GetAsync(AccountIdentifierKey);
        if (!string.IsNullOrWhiteSpace(identifier))
        {
            var account = await _authenticationClient.GetAccountAsync(identifier);

            if (account is not null)
            {
                await LoginAsync(cancellationToken, account);
            }
        }
    }

    public async Task LoginInteractiveAsync(CancellationToken cancellationToken)
    {
        await EnsureRegisteredCache();

        await LoginAsync(cancellationToken, null);
    }

    private async Task LoginAsync(CancellationToken cancellationToken, IAccount? account)
    {
        try
        {
            _secureStorage.Remove(AccountIdentifierKey);

            AuthenticationResult result;
            if (account is null)
            {
                result = await _authenticationClient
                        .AcquireTokenInteractive(Constants.Scopes)
                        .WithPrompt(Prompt.ForceLogin)
#if ANDROID
                .WithParentActivityOrWindow(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity)
#endif
#if WINDOWS
                .WithUseEmbeddedWebView(false)
#endif
                        .ExecuteAsync(cancellationToken);
            }
            else
            {
                result = await _authenticationClient
                    .AcquireTokenSilent(Constants.Scopes, account)
                    .ExecuteAsync(cancellationToken);
            }

            if (result.IdToken != null)
            {
                var idToken = result.IdToken; // you can also get AccessToken if you need it
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(idToken);
                if (jwtToken is not null)
                {
                    var claims = jwtToken.Claims.ToList();
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine($"Name: {claims.FirstOrDefault(x => x.Type.Equals("name"))?.Value}");
                    stringBuilder.AppendLine(
                        $"Email: {claims.FirstOrDefault(x => x.Type.Equals("preferred_username"))?.Value}");
                    Console.WriteLine(stringBuilder.ToString());
                    //LoginResultLabel.Text = stringBuilder.ToString();

                    if (result.Account is not null)
                    {
                        var identifier = result.Account.HomeAccountId?.Identifier;
                        if (identifier is not null)
                        {
                            await _secureStorage.SetAsync(AccountIdentifierKey, identifier);
                        }
                    }

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

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        CurrentUser = null;

        _secureStorage.Remove(AccountIdentifierKey);
        var accounts = await _authenticationClient.GetAccountsAsync();

        foreach (var account in accounts)
        {
            await _authenticationClient.RemoveAsync(account);
        }

        NotifyCurrentUserChanged();
    }

    private void NotifyCurrentUserChanged()
    {
        CurrentUserChanged?.Invoke(this, CurrentUser);
    }
}
