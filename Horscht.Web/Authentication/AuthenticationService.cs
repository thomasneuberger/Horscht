using System.Security.Claims;
using Horscht.App.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Horscht.Web.Authentication;

public class AuthenticationService : IAuthenticationService, IDisposable
{
    private readonly NavigationManager _navigationManager;

    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AuthenticationService(NavigationManager navigationManager, AuthenticationStateProvider authenticationStateProvider)
    {
        _navigationManager = navigationManager;
        _authenticationStateProvider = authenticationStateProvider;

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

    public async Task LoginAsync(CancellationToken cancellationToken)
    {
        _navigationManager.NavigateToLogin("authentication/login");

        await Task.CompletedTask;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        _navigationManager.NavigateToLogout("authentication/logout");

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _authenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
