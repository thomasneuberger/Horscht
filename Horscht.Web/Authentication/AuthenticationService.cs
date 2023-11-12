using System.Security.Claims;
using Horscht.Contracts.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Graph;
using AccessToken = Azure.Core.AccessToken;

namespace Horscht.Web.Authentication;

public class AuthenticationService : IAuthenticationService, IDisposable
{
    private readonly NavigationManager _navigationManager;

    private readonly AuthenticationStateProvider _authenticationStateProvider;

    private readonly IAccessTokenProvider _accessTokenProvider;

    public AuthenticationService(NavigationManager navigationManager, AuthenticationStateProvider authenticationStateProvider, IAccessTokenProvider accessTokenProvider)
    {
        _navigationManager = navigationManager;
        _authenticationStateProvider = authenticationStateProvider;
        _accessTokenProvider = accessTokenProvider;

        _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> authStateTask)
    {
        var authState = authStateTask.Result;
        CurrentUserChanged?.Invoke(this, authState.User.Identity as ClaimsIdentity);
    }

    public ClaimsIdentity? CurrentUser
    {
        get
        {
            var authState = _authenticationStateProvider.GetAuthenticationStateAsync().Result;
            return authState.User.Identity as ClaimsIdentity;
        }
    }

    public event EventHandler<ClaimsIdentity?>? CurrentUserChanged;

    public Task TryLoginSilentAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task LoginInteractiveAsync(CancellationToken cancellationToken)
    {
        _navigationManager.NavigateToLogin("authentication/login");

        await Task.CompletedTask;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        _navigationManager.NavigateToLogout("authentication/logout");

        await Task.CompletedTask;
    }

    public async Task<AccessToken?> GetAccessTokenAsync(string[] scopes, CancellationToken cancellationToken)
    {
        var options = new AccessTokenRequestOptions
        {
            Scopes = scopes
        };
        var result = await _accessTokenProvider.RequestAccessToken(options);

        if (result is not null)
        {
            if (result.TryGetToken(out var token))
            {
                return new AccessToken(token.Value, token.Expires);
            }
        }

        return null;
    }

    public void Dispose()
    {
        _authenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
