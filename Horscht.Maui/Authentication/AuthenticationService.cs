using Horscht.App.Authentication;
using Horscht.App.Services;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Horscht.Maui.Authentication;
public class AuthenticationService : IAuthenticationService
{
    private const string AccountIdentifierKey = "LoggedInAccount";
    private readonly IPublicClientApplication _authenticationClient;

    private readonly ISecureStorage _secureStorage;

    private MsalCacheHelper? _cacheHelper;

    public AuthenticationService(ISecureStorage secureStorage, IOptions<AuthenticationOptions> authenticationOptions)
    {
        _secureStorage = secureStorage;

        _authenticationClient = PublicClientApplicationBuilder.Create(authenticationOptions.Value.ClientId)
            .WithTenantId(authenticationOptions.Value.TenantId)
#if WINDOWS
            .WithRedirectUri("http://localhost")
#else
            .WithRedirectUri($"msal{authenticationOptions.Value.ClientId}://auth")
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

            _cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties)
                .ConfigureAwait(false);
            _cacheHelper.RegisterCache(_authenticationClient.UserTokenCache);
        }
    }

    public async Task TryLoginSilentAsync(CancellationToken cancellationToken)
    {
        var account = await GetAccountFromCache()
            .ConfigureAwait(false);

        if (account is not null)
        {
            await LoginAsync(account, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task LoginInteractiveAsync(CancellationToken cancellationToken)
    {
        await EnsureRegisteredCache()
            .ConfigureAwait(false);

        await LoginAsync(null, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<IAccount?> GetAccountFromCache()
    {
        await EnsureRegisteredCache()
            .ConfigureAwait(false);

        var identifier = await _secureStorage.GetAsync(AccountIdentifierKey)
            .ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(identifier))
        {
            var account = await _authenticationClient.GetAccountAsync(identifier)
                .ConfigureAwait(false);

            return account;
        }

        return null;
    }

    public async Task<string?> GetAccessTokenAsync(string[] scopes, CancellationToken cancellationToken)
    {
        var account = await GetAccountFromCache()
            .ConfigureAwait(false);

        var token = await AcquireTokenAsync(account, scopes, cancellationToken);

        return token.AccessToken;
    }

    private async Task LoginAsync(IAccount? account, CancellationToken cancellationToken)
    {
        try
        {
            _secureStorage.Remove(AccountIdentifierKey);

            var result = await AcquireTokenAsync(account, AuthenticationConstants.Scopes, cancellationToken)
                .ConfigureAwait(false);

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
                            await _secureStorage.SetAsync(AccountIdentifierKey, identifier)
                                .ConfigureAwait(false);
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

    private async Task<AuthenticationResult> AcquireTokenAsync(IAccount? account, string[] scopes, CancellationToken cancellationToken)
    {
        AuthenticationResult result;
        if (account is null)
        {
            result = await _authenticationClient
                .AcquireTokenInteractive(scopes)
                .WithPrompt(Prompt.ForceLogin)
#if ANDROID
                .WithParentActivityOrWindow(Platform.CurrentActivity)
#endif
#if WINDOWS
                .WithUseEmbeddedWebView(false)
#endif
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            try
            {
                result = await _authenticationClient
                    .AcquireTokenSilent(scopes, account)
                    .ExecuteAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (MsalUiRequiredException)
            {
                result = await _authenticationClient
                    .AcquireTokenInteractive(scopes)
                    .WithLoginHint(account.Username)
                    .ExecuteAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return result;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        CurrentUser = null;

        _secureStorage.Remove(AccountIdentifierKey);
        var accounts = await _authenticationClient.GetAccountsAsync();

        foreach (var account in accounts)
        {
            await _authenticationClient.RemoveAsync(account)
                .ConfigureAwait(false);
        }

        NotifyCurrentUserChanged();
    }

    private void NotifyCurrentUserChanged()
    {
        CurrentUserChanged?.Invoke(this, CurrentUser);
    }
}
